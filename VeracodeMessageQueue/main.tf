provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "veracode" {
  name     = "${var.prefix}-resources"
  location = var.location
}

resource "azurerm_eventgrid_topic" "veracode" {
  name                = "${var.prefix}-eventgrid-topic"
  location            = var.location
  resource_group_name = "${var.prefix}-resources"
}