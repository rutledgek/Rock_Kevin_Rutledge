using System;

namespace online.kevinrutledge.InvoiceSystem
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CSSColorAttribute : Attribute
    {
        public string Color { get; }

        public CSSColorAttribute(string color)
        {
            Color = color;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class LabelTypeAttribute : Attribute
    {
        public Rock.Web.UI.Controls.LabelType LabelType { get; }

        public LabelTypeAttribute(Rock.Web.UI.Controls.LabelType labelType)
        {
            LabelType = labelType;
        }
    }
}