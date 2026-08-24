// Runs on Jenkins-in-EKS via the Kubernetes plugin — each stage below that needs a specific
// toolchain requests an ephemeral pod (see assura-gitops/apps/03-jenkins.yaml podTemplates),
// scaling build capacity to zero when nothing is building. This pipeline builds, scans, and
// pushes an image, then edits ONE line in assura-gitops and stops — it never runs kubectl or
// touches the cluster. ArgoCD is the only thing that deploys.
//
// Every stage below starts with `checkout scm`: the Kubernetes plugin allocates a fresh pod per
// `agent { label ... }` block (workspaces are not shared across stages just because the label
// matches), so each stage that needs the source re-checks it out rather than assuming a prior
// stage's checkout or build output is still on disk.
pipeline {
    agent none

    environment {
        ECR_REPO    = "CHANGE_ME.dkr.ecr.us-east-1.amazonaws.com/assura-demo-backend"
        IMAGE_TAG   = "${env.GIT_COMMIT.take(12)}"
        GITOPS_REPO = "https://github.com/System-Street-Studio/assura-gitops.git"
    }

    stages {
        stage('Secret scan') {
            agent { label 'dotnet-sdk' }
            steps {
                checkout scm
                // Fails fast, before any build compute is spent, matching Phase 0's
                // pre-commit gitleaks gate — this is the same check re-run server-side so a
                // secret that bypassed the local hook still can't reach an image.
                sh 'curl -sSfL https://raw.githubusercontent.com/gitleaks/gitleaks/master/install.sh | sh -s -- -b /tmp v8.21.2'
                sh '/tmp/gitleaks detect --source . --redact --exit-code 1 --config .gitleaks.toml'
            }
        }

        stage('Build') {
            agent { label 'dotnet-sdk' }
            steps {
                checkout scm
                sh 'dotnet restore AssuraBackend.sln'
                sh 'dotnet build AssuraBackend.sln -c Release --no-restore'
            }
        }

        stage('SAST — Semgrep') {
            agent { label 'dotnet-sdk' }
            steps {
                checkout scm
                // mcr.microsoft.com/dotnet/sdk doesn't ship python3 — installed here rather
                // than assuming it's present (verified: it isn't, on the 8.0 image as of this
                // writing).
                sh 'apt-get update -qq && apt-get install -y -qq python3-pip'
                sh 'pip install --quiet --break-system-packages semgrep'
                sh 'semgrep scan --config p/csharp --error --sarif --output semgrep-backend.sarif .'
            }
            post {
                always { archiveArtifacts artifacts: 'semgrep-backend.sarif', allowEmptyArchive: true }
            }
        }

        stage('SCA — vulnerable packages') {
            agent { label 'dotnet-sdk' }
            steps {
                checkout scm
                sh 'dotnet restore AssuraBackend.sln'
                // Fails the build on any known-vulnerable NuGet package, transitive included —
                // exactly the gate the deployment plan's verification demo exercises by
                // deliberately pinning a CVE-affected package version.
                sh '''
                    dotnet list AssuraBackend.sln package --vulnerable --include-transitive 2>&1 | tee vuln-report.txt
                    if grep -q "has the following vulnerable packages" vuln-report.txt; then
                        echo "Vulnerable package(s) found — failing the build."
                        exit 1
                    fi
                '''
            }
        }

        stage('Unit tests') {
            agent { label 'dotnet-sdk' }
            steps {
                checkout scm
                // Application/API test projects use EF Core InMemory (see
                // tests/Assura.Application.Tests, tests/Assura.API.Tests) — this never touches
                // a real database, remote or otherwise. No --no-build: this stage's pod never
                // saw the earlier "Build" stage's output, so `dotnet test` does its own build.
                sh 'dotnet test AssuraBackend.sln -c Release --collect:"XPlat Code Coverage"'
            }
            post {
                always { junit '**/TestResults/**/*.trx' }
            }
        }

        stage('Build & push image') {
            agent { label 'kaniko' }
            steps {
                checkout scm
                // Kaniko builds from a Dockerfile with no privileged container or mounted
                // Docker socket. ECR auth comes from this pod's own IRSA role — no static AWS
                // keys anywhere in Jenkins.
                sh """
                    /kaniko/executor \
                      --context=. \
                      --dockerfile=Dockerfile \
                      --destination=${ECR_REPO}:${IMAGE_TAG} \
                      --cache=true
                """
            }
        }

        stage('Image scan — Trivy') {
            agent { label 'trivy' }
            steps {
                // No checkout needed: scans the already-pushed image by reference, not local source.
                sh "trivy image --exit-code 1 --severity HIGH,CRITICAL ${ECR_REPO}:${IMAGE_TAG}"
            }
        }

        stage('SBOM + sign') {
            agent { label 'syft-cosign' }
            steps {
                // No checkout needed: both tools operate on the already-pushed image by
                // reference. The syft-cosign pod runs plain Alpine (the official syft/cosign
                // images are shell-less/distroless, verified directly — the usual
                // sleep-as-keep-alive pattern doesn't work on them), so both binaries are
                // fetched directly here first, same as the yq install in the gitops-update stage.
                sh 'wget -qO /tmp/syft.tar.gz https://github.com/anchore/syft/releases/download/v1.18.0/syft_1.18.0_linux_amd64.tar.gz && tar -xzf /tmp/syft.tar.gz -C /usr/local/bin syft'
                sh 'wget -qO /usr/local/bin/cosign https://github.com/sigstore/cosign/releases/download/v2.4.1/cosign-linux-amd64 && chmod +x /usr/local/bin/cosign'
                sh "syft ${ECR_REPO}:${IMAGE_TAG} -o cyclonedx-json > sbom-backend.json"
                // Signs via AWS KMS through this pod's own IRSA role, scoped to kms:Sign on
                // exactly one key ARN (see assura-infra/modules/iam-irsa) — no cosign.key file
                // ever exists on disk to leak.
                sh "cosign sign --key awskms:///CHANGE_ME_COSIGN_KEY_ARN ${ECR_REPO}:${IMAGE_TAG}"
            }
            post {
                always { archiveArtifacts artifacts: 'sbom-backend.json', allowEmptyArchive: true }
            }
        }

        stage('Update GitOps manifest') {
            agent { label 'git' }
            steps {
                // Clones a DIFFERENT repo (assura-gitops), not this one — no checkout scm here.
                // The only step that touches assura-gitops, and the only field it ever edits is
                // image.tag — everything else in that repo is a human/ArgoCD concern, never CI's.
                sh 'wget -qO /usr/local/bin/yq https://github.com/mikefarah/yq/releases/download/v4.44.6/yq_linux_amd64 && chmod +x /usr/local/bin/yq'
                sh 'apk add --no-cache aws-cli'
                // Fully single-quoted (no Groovy string interpolation anywhere in this block):
                // GIT_TOKEN, GITOPS_REPO, and IMAGE_TAG are all resolved as plain shell
                // variables at runtime instead, so the token never passes through Jenkins'
                // Groovy compilation/replay layer and is never echoed. This pod's own IRSA role
                // (see assura-gitops's Jenkins Application) can read only this one Secrets
                // Manager entry — nothing else.
                sh '''
                    set -eu
                    GIT_TOKEN=$(aws secretsmanager get-secret-value \
                        --secret-id assura-demo/github-gitops-pat \
                        --query SecretString --output text --region us-east-1 \
                        | yq -r '.token')
                    git clone "https://x-access-token:${GIT_TOKEN}@${GITOPS_REPO#https://}" gitops
                    cd gitops
                    yq -i '.image.tag = strenv(IMAGE_TAG)' charts/assura-backend/values-image.yaml
                    git config user.name "jenkins-bot"
                    git config user.email "jenkins-bot@assura.local"
                    git commit -am "deploy: assura-backend@${IMAGE_TAG}"
                    git push origin main
                '''
            }
        }
    }
}
