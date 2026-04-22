# Azure Infrastructure

This folder stores Azure infrastructure deployment files for OneJax.

Files:
- `onejax.template.json`: ARM template for Azure resources
- `onejax.example.parameters.json`: safe example parameters file

The App Service name in these files is aligned to:
- `OneJaxStrategicDashboard`

Do not commit real secrets to this repo.

In particular, keep `sqlAdministratorPassword` out of source control.
Use one of these instead:
- Azure CLI `--parameters sqlAdministratorPassword=<value>`
- GitHub Actions secrets
- Azure Key Vault

If you want a real local parameters file, create one such as:
- `infra/azure/onejax.parameters.json`

That file is ignored by Git.
