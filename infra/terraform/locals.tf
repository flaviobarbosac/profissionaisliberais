locals {
  api_fqdn    = "${var.api_subdomain}.${var.domain_name}"
  aspire_fqdn = "${var.aspire_subdomain}.${var.domain_name}"
  front_fqdn  = "${var.front_subdomain}.${var.domain_name}"
  name_prefix = "${var.project}-${var.environment}"
}
