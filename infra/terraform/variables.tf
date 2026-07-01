variable "aws_region" {
  type    = string
  default = "sa-east-1"
}

variable "project" {
  type    = string
  default = "libify"
}

variable "environment" {
  type    = string
  default = "dev"
}

variable "domain_name" {
  type    = string
  default = "libify.com.br"
}

variable "api_subdomain" {
  type    = string
  default = "api.dev"
}

variable "aspire_subdomain" {
  type    = string
  default = "aspire.dev"
}

variable "front_subdomain" {
  type    = string
  default = "www.dev"
}

variable "db_name" {
  type    = string
  default = "libify_dev"
}

variable "db_username" {
  type    = string
  default = "libify_admin"
}

variable "ec2_instance_type" {
  type    = string
  default = "t4g.small"
}

variable "rds_instance_class" {
  type    = string
  default = "db.t4g.micro"
}

variable "allowed_ssh_cidr" {
  type        = string
  default     = ""
  description = "CIDR opcional para SSH. Vazio = SSH fechado (usar SSM)."
}

variable "github_repository" {
  type        = string
  default     = "https://github.com/flaviobarbosac/prosiffionaisliberais-front"
  description = "Repositorio GitHub do front (Amplify)."
}

variable "github_access_token" {
  type        = string
  default     = ""
  sensitive   = true
  description = "GitHub PAT (repo + admin:repo_hook). Vazio = app sem repo conectado."
}

variable "google_client_id" {
  type        = string
  default     = "560156263793-94lhftrcshishb58en2tqtjgatmv31b9.apps.googleusercontent.com"
  description = "OAuth Client ID Google (front VITE_GOOGLE_CLIENT_ID + API Google:ClientId)."
}

variable "github_branch" {
  type    = string
  default = "develop"
}

variable "aspire_allowed_cidr" {
  type        = string
  default     = "0.0.0.0/0"
  description = "Restringir IP para aspire.dev (recomendado seu IP fixo)."
}
