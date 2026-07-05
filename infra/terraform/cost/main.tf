# FinOps / cost dashboard for ClauseLens.
# Source: OpenAI token usage + Azure resource spend.
# Note: requires Log Analytics workspace + Cost Management exports to be
# configured at the subscription level. This file declares the dashboard
# only; the data feed is wired in a follow-up.

terraform {
  required_providers {
    azurerm = { source = "hashicorp/azurerm", version = "~> 4.0" }
  }
}

variable "resource_group_name" { type = string }

# Token usage & cost table
resource "azurerm_log_analytics_workspace" "finops" {
  name                = "clauselens-finops"
  resource_group_name = var.resource_group_name
  location            = "eastus2"
  sku                 = "PerGB2018"
  retention_in_days   = 90
}

output "finops_workspace_id" { value = azurerm_log_analytics_workspace.finops.id }
