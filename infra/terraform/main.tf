terraform {
  required_version = ">= 1.9.0"
  required_providers {
    azurerm = { source = "hashicorp/azurerm", version = "~> 4.0" }
    azuread = { source = "hashicorp/azuread", version = "~> 3.0" }
  }
}

provider "azurerm" {
  features {}
}

variable "project" { type = string, default = "clauselens" }
variable "environment" { type = string, default = "dev" }
variable "location" { type = string, default = "eastus2" }

resource "azurerm_resource_group" "main" {
  name     = "${var.project}-${var.environment}-rg"
  location = var.location
}

# AKS cluster (Standard SKU; private API server; Azure CNI Overlay)
module "aks" {
  source  = "Azure/aks/azurerm"
  version = "~> 9.0"

  name                = "${var.project}-${var.environment}-aks"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  prefix              = "${var.project}-${var.environment}"

  network_plugin     = "azure"
  network_plugin_mode = "overlay"
  network_policy     = "cilium"
  private_cluster_enabled = true
  api_server_authorized_ip_ranges = []

  rbac_aad_managed = true
  role_based_access_control_enabled = true
  entra_id_enabled = true
  identity_type    = "SystemAssigned"

  depends_on = [azurerm_resource_group.main]
}

# SQL Server + database
resource "azurerm_mssql_server" "main" {
  name                         = "${var.project}-${var.environment}-sql"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = "clauselens"
  administrator_login_password = "REPLACE_ME_VIA_KEYVAULT"
  minimum_tls_version          = "1.2"
  public_network_access_enabled = false
}

resource "azurerm_mssql_database" "main" {
  name      = "ClauseLens"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "S0"
  max_size_gb = 32
}

# Azure AI Search (hybrid vector + keyword)
resource "azurerm_search_service" "main" {
  name                = "${var.project}-${var.environment}-search"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  sku                 = "standard"
  partition_count     = 1
  replica_count       = 1
  public_network_access_enabled = true
}

# Service Bus namespace
resource "azurerm_servicebus_namespace" "main" {
  name                = "${var.project}-${var.environment}-sb"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  sku                 = "Standard"
}

# Key Vault
resource "azurerm_key_vault" "main" {
  name                = "${var.project}-${var.environment}-kv"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"
  enable_rbac_authorization = true
  public_network_access_enabled = false
}

data "azurerm_client_config" "current" {}

output "resource_group_name" { value = azurerm_resource_group.main.name }
output "aks_name"           { value = module.aks.name }
output "sql_server_fqdn"    { value = azurerm_mssql_server.main.fully_qualified_domain_name }
output "search_endpoint"    { value = "https://${azurerm_search_service.main.name}.search.windows.net" }
output "servicebus_endpoint" { value = azurerm_servicebus_namespace.main.endpoint }
