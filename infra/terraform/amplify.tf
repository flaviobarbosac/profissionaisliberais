resource "aws_amplify_app" "front" {
  name        = "${local.name_prefix}-front"
  platform    = "WEB"
  description = "Libify front dev"
  build_spec  = file("${path.module}/templates/amplify.yml")

  repository   = var.github_access_token != "" ? var.github_repository : null
  access_token = var.github_access_token != "" ? var.github_access_token : null

  enable_branch_auto_build = var.github_access_token != ""

  environment_variables = {
    VITE_API_BASE_URL       = "https://${local.api_fqdn}/api/v1"
    VITE_APP_VERSION        = "0.1.0-dev"
    VITE_GOOGLE_CLIENT_ID   = var.google_client_id
  }

  custom_rule {
    source = "/<*>"
    status = "404-200"
    target = "/index.html"
  }
}

resource "aws_amplify_branch" "dev" {
  app_id      = aws_amplify_app.front.id
  branch_name = var.github_branch
  framework   = "React"
  stage       = "DEVELOPMENT"

  enable_auto_build = var.github_access_token != ""

  environment_variables = {
    VITE_API_BASE_URL     = "https://${local.api_fqdn}/api/v1"
    VITE_GOOGLE_CLIENT_ID = var.google_client_id
  }
}

# Dominio customizado www.dev.libify.com.br: configurar no console Amplify
# apos delegacao NS no Registro.br (CNAME de verificacao + CloudFront)
