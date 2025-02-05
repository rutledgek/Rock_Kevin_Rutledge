
namespace online.kevinrutledge.InvoiceSystem.SystemGuids
{
    #region List of Guids for the Invoice System Plugin
    public static class EntityTypeGuids
    {
        public const string Invoice_Type = "C359C6F6-44A9-455F-8513-2903D33BFF2D";
        public const string Invoice = "279E1509-6BB2-40E8-8250-48AD495537B0";
        public const string Invoice_Item = "320C299F-2E37-4F09-97BB-AF808969C580";
        public const string Invoice_Assignment = "9CFC5619-DE7B-40E8-AFF8-BD09CF024F59";
        public const string Scheduled_Invoice = "DF14A0C8-591C-4DE7-A8EA-8941215755A2";
        public const string ScheduledInvoice_Assignment = "9F4E0987-183E-4A9C-99A0-88D355F49679";
        public const string ScheduledInvoice_Item = "0E194177-D3AE-42A5-93E8-02F6EB7F4C28";

    }

  
    public static class PageGuids
    {

        public const string InvoiceSystemParentPage = "50AA4AB1-873D-48CB-B47E-9C39F493B2A9";
        public const string InvoiceTypeListPage = "B13741EE-8F2D-4269-A9D1-6020E4F52730";
        public const string InvoiceTypeDetailPage = "75E3F4CF-E8F7-4D1E-88A0-F454657338C9";
        public const string InvoiceListPage = "E1D051DE-17AF-49BD-B2B2-1EEE73C18D64";
        public const string InvoiceDetailPage = "4CDBBB54-F4F3-4614-B240-E73FB6B3D679";
        public const string ScheduledInvoiceListPage = "3196A7D3-8D89-4BB1-8FD6-AD3CCB3513DF";
        public const string ScheduledInvoiceDetailPage = "C299A8F5-3FED-43A5-A8D0-DDA77F9008E4";
    }

    public static class Categories
    {
        public const string InvoiceSystemCommumincations = "17AFC761-9AEA-4AA8-B9A3-0C465FEB3EC0";
        public const string InvoiceSystemInvoceCommunication = "0DCB034C-CCEC-4F44-806B-46F5294534C3";
        public const string InvoiceSystemLateNoticeCommunication = "A72A3E1C-08C0-4FEA-B3C2-DD264459DB60";

    }

    public static class BlockTypeGuids
    {
        public const string InvoiceTypeList = "71EA18D3-0086-421B-8059-DF00630DBA7B";
        public const string InvoiceTypeDetail = "A03E8FB4-34C6-4FCD-B263-B8775BC37BE4";
        public const string InvoiceList = "BCAD5374-C2AC-40CD-8FD9-867913394978";
        public const string InvoiceDetail = "61AAAEAE-BFA5-4DF8-A9AF-E9B7D01A5372";
        public const string ScheduledInvoiceList = "5CDB3437-6367-4810-A45D-E7B75697A19C";
        public const string ScheduledInvoiceDetail = "611D7478-3130-45CB-B47A-6D2F99FD839A";

    }

    public static class BlockGuids
    {
        public const string InvoiceTypeListBlock = "2C71E4B7-1E55-47A6-BC0E-3FE1CAB85AA9";
        public const string InvoiceTypeDetailBlock = "B03CAC59-D059-48BD-9761-ECF8A7C81026";
        public const string InvoiceListBlock = "1782A86D-5595-4056-87B5-A7D2567D1428";
        public const string InvoiceDetailBlock = "CCFD1C06-389D-44AE-BBA0-1797B43B8103";
        public const string ScheduledInvoiceListBlock = "15970777-5994-44A6-BECC-83ACAA3F81D6";
        public const string ScheduledInvoiceDetailBlock = "77C4A9C5-08E2-433D-ACB1-E12668069A81";
    }


    #endregion


    #region System Guids Do not Change or Delete
    public static class SystemGuids
    {
        public const string InstalledPluginsPage = "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616";
        public const string FullWidthLayout = "D65F783D-87A9-4CC9-8110-E83466A0EADB";
        public const string PageMenuBlockType = "CACB9D1A-A820-4587-986A-D66A69EE9948";
        public const string PageMenuTemplateContent = "1322186A-862A-4CF1-B349-28ECB67229BA";
    }

    public static class AttributeContent
    {
        public const string SubMenuContent = "{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}";
    }

    #endregion
}
