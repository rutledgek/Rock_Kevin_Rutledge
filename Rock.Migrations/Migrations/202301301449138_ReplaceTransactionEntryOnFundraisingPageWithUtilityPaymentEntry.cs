// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    public partial class ReplaceTransactionEntryOnFundraisingPageWithUtilityPaymentEntry : Rock.Migrations.RockMigration
    {
        const string finishLavaTemplate = @"
<div class=""well"">

    <legend>Donation Information</legend>

    {% assign groupMember = TransactionEntity %}
    
    <div class=""row"">
        <div class=""col-md-6"">
          <dl>
            <dt>Transaction Date</dt>
            <dd>
                {{ Transaction.TransactionDateTime }}
            </dd>
          </dl>
        </div>
        <div class=""col-md-6"">
        </div>
    </div>        
    
    <div class=""row"">
        <div class=""col-md-6"">
            <dl>
                <dt>Fundraising Opportunity</dt>
                <dd>{{ groupMember.Group | Attribute:''OpportunityTitle'' }}</dd>
            </dl>
        </div>
        <div class=""col-md-6"">
            <dl>
                <dt>Participant</dt>
                <dd>{{ groupMember.Person.FullName }}</dd>
            </dl>
        </div>
    </div>    
    <p>Thank you for your contribution for this {{ groupMember.Group | Attribute:''OpportunityType'' }}. We are grateful for your commitment.</p>

    {% if Transaction.ScheduledTransactionDetails %}
        {% assign transactionDetails = Transaction.ScheduledTransactionDetails %}
    {% else %}
        {% assign transactionDetails = Transaction.TransactionDetails %}
    {% endif %}
    
    <dl class=""dl-horizontal"">
        <dt>Confirmation Code</dt>
        <dd>{{ Transaction.TransactionCode }}</dd>
        <dd></dd>
    </dl>
    
    <dl class=""dl-horizontal gift-success"">
        <dt>Name</dt>
        <dd>{{ Person.FullName }}</dd>
        <dd></dd>
        
        <dt>Email</dt>
        <dd>{{ Person.Email }}</dd>
        <dd></dd>
        
        <dt>Address</dt>
        <dd>{{ BillingLocation.Street }} {{ BillingLocation.City }}, {{ BillingLocation.State }} {{ BillingLocation.PostalCode }}</dd>
        <dd></dd>
    </dl>
    
    <dl class=""dl-horizontal"">
        {% for transactionDetail in transactionDetails %}
            <dt>{{ transactionDetail.Account.PublicName }}</dt>
            <dd>{{ transactionDetail.Amount | Minus: transactionDetail.FeeCoverageAmount | FormatAsCurrency }}</dd>
        {% endfor %}
            <dt>Total</dt>
            <dd>{{ transactionDetails | Select: ''Amount'' | Sum | FormatAsCurrency }}</dd>
        {% if Transaction.TotalFeeCoverageAmount %}
            <dt>Fee Coverage</dt>
            <dd>{{ Transaction.TotalFeeCoverageAmount | FormatAsCurrency }}</dd>
        {% endif %}
        <dd></dd>
    </dl>
    
    <dl class=""dl-horizontal"">
            <dt>Payment Method</dt>
        <dd>{{ PaymentDetail.CurrencyTypeValue.Description}}</dd>
    
        {% if PaymentDetail.AccountNumberMasked  != '' %}
            <dt>Account Number</dt>
            <dd>{{ PaymentDetail.AccountNumberMasked }}</dd>
        {% endif %}
    
        <dt>When<dt>
        <dd>
    
        {% if Transaction.TransactionFrequencyValue %}
            {{ Transaction.TransactionFrequencyValue.Value }} starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}
        {% else %}
            Today
        {% endif %}
        </dd>
    </dl>
</div>
";
        const string setUpSqlFormat = @"
DECLARE @PageId INT = (SELECT Id FROM PAGE WHERE [Guid] = '{0}')
DECLARE @EntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
DECLARE @BlockToBeReplacedBlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{1}')
DECLARE @BlockToBeReplacedBlockId INT = (SELECT [Id] FROM [Block] WHERE [BlockTypeId] = @BlockToBeReplacedBlockTypeId AND [PageId] = @PageId)
DECLARE @NewBlockBlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{2}')

/* Add New Block instance to page */
IF NOT EXISTS (select * from [Block] where [Guid] = '{3}')
BEGIN			
	INSERT INTO [Block] (
	    [IsSystem],[PageId],[BlockTypeId],[Zone],
	    [Order],[Name],[PreHtml],[PostHtml],[OutputCacheDuration],
	    [Guid])
	VALUES(
	    1,@PageId,@NewBlockBlockTypeId,'Main',
	    0,'{4}','','',0,
	    '{3}')

END

DECLARE @NewBlockBlockId AS INT = (SELECT Id FROM Block WHERE Guid = '{3}')
DECLARE @NewBlockBlockAttributeId AS INT
DECLARE @NewBlockBlockAttributeValueId AS INT
DECLARE @BlockToBeReplacedBlockAttributeId AS INT
DECLARE @BlockToBeReplacedBlockAttributeValueId AS INT
DECLARE @BlockToBeReplacedBlockAttributeValue AS NVARCHAR(MAX)
";
        const string copyAttributeValueSqlFormat = @"

SET @BlockToBeReplacedBlockAttributeId = (SELECT Id FROM [Attribute]
	WHERE EntityTypeId = @EntityTypeId
	AND EntityTypeQualifierValue = @BlockToBeReplacedBlockTypeId
	AND EntityTypeQualifierColumn = 'BlockTypeId' and [Key] = '{1}')

SET @BlockToBeReplacedBlockAttributeValueId = (SELECT Id FROM AttributeValue
	WHERE EntityId = @BlockToBeReplacedBlockId
	AND AttributeId = @BlockToBeReplacedBlockAttributeId)

IF (@BlockToBeReplacedBlockAttributeValueId IS NOT NULL)
BEGIN

    SET @NewBlockBlockAttributeId = (SELECT Id FROM [Attribute]
	    WHERE EntityTypeId = @EntityTypeId
	    AND EntityTypeQualifierValue = @NewBlockBlockTypeId
	    AND EntityTypeQualifierColumn = 'BlockTypeId' and [Key] = '{0}')

    SET @NewBlockBlockAttributeValueId = (SELECT Id FROM [AttributeValue]
        WHERE [AttributeId] = @NewBlockBlockAttributeId
        AND [EntityId] = @NewBlockBlockId)

    SET @BlockToBeReplacedBlockAttributeValue = (SELECT Value FROM AttributeValue
    	WHERE Id = @BlockToBeReplacedBlockAttributeValueId)

    IF @NewBlockBlockAttributeValueId IS NULL
    BEGIN
    	INSERT INTO [AttributeValue] (
    		[IsSystem],[AttributeId],[EntityId],
    		[Value],
    		[Guid])
    	VALUES(
    		1,@NewBlockBlockAttributeId,@NewBlockBlockId,
    		@BlockToBeReplacedBlockAttributeValue,
    		NEWID())
    END
    ELSE
    BEGIN
    	UPDATE [AttributeValue]
    	SET [Value] = @BlockToBeReplacedBlockAttributeValue
    	WHERE Id = @NewBlockBlockAttributeValueId
    END

END
";
        const string createOrUpdateAttributeValueSqlFormat = @"
SET @NewBlockBlockAttributeId = (SELECT Id FROM [Attribute]
	WHERE EntityTypeId = @EntityTypeId
	AND EntityTypeQualifierValue = @NewBlockBlockTypeId
	AND EntityTypeQualifierColumn = 'BlockTypeId' and [Key] = '{0}')

SET @NewBlockBlockAttributeValueId = (SELECT Id FROM [AttributeValue]
    WHERE [AttributeId] = @NewBlockBlockAttributeId
    AND [EntityId] = @NewBlockBlockId)

IF @NewBlockBlockAttributeValueId IS NULL
BEGIN
	INSERT INTO [AttributeValue] (
		[IsSystem],[AttributeId],[EntityId],
		[Value],
		[Guid])
	VALUES(
		1,@NewBlockBlockAttributeId,@NewBlockBlockId,
		'{1}',
		NEWID())
END
ELSE
BEGIN
	UPDATE [AttributeValue]
	SET [Value] = '{1}'
	WHERE Id = @NewBlockBlockAttributeValueId
END
";
        const string deleteOldBlockSql = @"
DELETE [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @BlockToBeReplacedBlockId
DELETE [Block] WHERE [Id] = @BlockToBeReplacedBlockId
";
        private readonly List<AttributeKeyMapping> attributesToCopy = new List<AttributeKeyMapping>()
        {
            // Attributes with different default values for this page
            new AttributeKeyMapping( "TransactionEntityType" ),
            new AttributeKeyMapping( "EntityIdParam" ),
            new AttributeKeyMapping( "TransactionHeader" ),
            new AttributeKeyMapping( "PersonalInfoTitle" ),
            new AttributeKeyMapping( "ConfirmationFooter" ),
            new AttributeKeyMapping( "AccountHeaderTemplate" ),
            new AttributeKeyMapping( "EnableInitialBackbutton" ),
            new AttributeKeyMapping( "BatchNamePrefix" ),
            //new AttributeKeyMapping( "AdditionalAccounts" ),
            new AttributeKeyMapping( "AllowScheduled" ),
            new AttributeKeyMapping( "EnableBusinessGiving" ),
            new AttributeKeyMapping( "EnableAnonymousGiving" ),

            // Other attributes incase block has some custom values set by user
            new AttributeKeyMapping( "Source" ),
            new AttributeKeyMapping( "Impersonation" ),
            new AttributeKeyMapping( "LayoutStyle" ),
            new AttributeKeyMapping( "DisplayPhone" ),
            new AttributeKeyMapping( "DisplayEmail" ),
            new AttributeKeyMapping( "AddressType" ),
            new AttributeKeyMapping( "ConnectionStatus" ),
            new AttributeKeyMapping( "RecordStatus" ),
            new AttributeKeyMapping( "EnableCommentEntry" ),
            new AttributeKeyMapping( "CommentEntryLabel" ),
            new AttributeKeyMapping( "ConfirmAccountTemplate" ),
            new AttributeKeyMapping( "ReceiptEmail" ),
            new AttributeKeyMapping( "PanelTitle" ),
            new AttributeKeyMapping( "ContributionInfoTitle" ),
            new AttributeKeyMapping( "PaymentInfoTitle" ),
            new AttributeKeyMapping( "ConfirmationTitle" ),
            new AttributeKeyMapping( "SaveAccountTitle" ),
            new AttributeKeyMapping( "ConfirmationHeader" ),
            new AttributeKeyMapping( "SuccessFooter" ),
            new AttributeKeyMapping( "AnonymousGivingTooltip" ),
            new AttributeKeyMapping( "OnlyPublicAccountsInURL" ),
            new AttributeKeyMapping( "InvalidAccountMessage" ),
            new AttributeKeyMapping( "AccountCampusContext" ),
            new AttributeKeyMapping( "AllowedTransactionAttributesFromURL" ),

            // Attributes with different keys but same functionalities
            new AttributeKeyMapping( "CCGateway", "FinancialGateway"  ),
            new AttributeKeyMapping( "PaymentComment", "PaymentCommentTemplate"  ),
            new AttributeKeyMapping( "AllowAccountsInURL", "AllowAccountOptionsInURL"  ),
            new AttributeKeyMapping( "Accounts", "AccountsToDisplay"  ),
        };

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            List<BlockSwapMigrationHelperClass> helperClasses = new List<BlockSwapMigrationHelperClass>
            {
                new BlockSwapMigrationHelperClass()
                {
                    AttributesToCopy = attributesToCopy,
                    AttributesToAdd = new Dictionary<string, string>()
                    {
                        { "PaymentCommentTemplate", "Fundraising" },
                        { "FinishLavaTemplate", finishLavaTemplate },
                    },
                    NewBlockBlockTypeGuid = "4CCC45A5-4AB9-4A36-BF8D-A6E316790004",
                    NewBlockGuid = "E11894E6-ABA7-4AD3-A7A2-89A2635314D4",
                    NewBlockName = "Fundraising Transaction Entry",
                    OldBlockTypeId = "74EE3481-3E5A-4971-A02E-D463ABB45591",
                    PageGuid = "F04D69C1-786A-4204-8A67-5669BDFEB533",
                }
            };

            foreach ( var helperClass in helperClasses )
            {
                var setUpSql = string.Format( setUpSqlFormat,
                    helperClass.PageGuid,// 0
                    helperClass.OldBlockTypeId,// 1 
                    helperClass.NewBlockBlockTypeGuid,// 2
                    helperClass.NewBlockGuid,// 3
                    helperClass.NewBlockName// 4
                    );
                var stringBuilder = new StringBuilder( setUpSql );

                if ( helperClass.AttributesToCopy != null )
                {
                    foreach ( var attributeKey in helperClass.AttributesToCopy )
                    {
                        stringBuilder.AppendFormat( copyAttributeValueSqlFormat, attributeKey.NewBlockKey, attributeKey.OldBlockKey );
                    }
                }

                if ( helperClass.AttributesToAdd != null )
                {
                    foreach ( var newAttributeValue in helperClass.AttributesToAdd )
                    {
                        stringBuilder.AppendFormat( createOrUpdateAttributeValueSqlFormat, newAttributeValue.Key, newAttributeValue.Value );
                    }
                }

                stringBuilder.AppendLine( deleteOldBlockSql );

                Sql( stringBuilder.ToString() );
            }
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "E11894E6-ABA7-4AD3-A7A2-89A2635314D4" );
        }

        private sealed class BlockSwapMigrationHelperClass
        {
            public string PageGuid { get; set; }
            public string NewBlockName { get; set; }
            public string NewBlockGuid { get; set; }
            public string NewBlockBlockTypeGuid { get; set; }
            public string OldBlockTypeId { get; set; }

            public List<AttributeKeyMapping> AttributesToCopy { get; set; }
            public Dictionary<string, string> AttributesToAdd { get; set; }
        }

        private sealed class AttributeKeyMapping
        {
            public AttributeKeyMapping( string key ) : this( key, key )
            {
            }

            public AttributeKeyMapping( string oldBlockKey, string newBlockKey )
            {
                OldBlockKey = oldBlockKey;
                NewBlockKey = newBlockKey;
            }

            public string OldBlockKey { get; set; }
            public string NewBlockKey { get; set; }
        }
    }
}
