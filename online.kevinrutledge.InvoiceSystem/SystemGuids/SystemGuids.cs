
namespace online.kevinrutledge.InvoiceSystem.SystemGuids
{
    #region List of Guids for the Invoice System Plugin
    public static class EntityTypeGuids
    {
        public const string Invoice_Type = "C359C6F6-44A9-455F-8513-2903D33BFF2D";

    }

  
    public static class PageGuids
    {

        public const string InvoiceSystemParentPage = "50AA4AB1-873D-48CB-B47E-9C39F493B2A9";
        public const string InvoiceTypeListPage = "B13741EE-8F2D-4269-A9D1-6020E4F52730";
        public const string InvoiceTypeDetailpage = "75E3F4CF-E8F7-4D1E-88A0-F454657338C9";
    }


    public static class BlockTypeGuids
    {
        public const string InvoiceTypeList = "71EA18D3-0086-421B-8059-DF00630DBA7B";

    }

    public static class BlockGuids
    {
        public const string InvoiceTypeListBlock = "2C71E4B7-1E55-47A6-BC0E-3FE1CAB85AA9";
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
