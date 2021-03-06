﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.269
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Conference.Specflow.Features.UserInterface.Views.Registration
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class SelfRegistrantEndToEndScenarioForMakingARegistrationFreeOfChargeForAConferenceSiteFeature : Xunit.IUseFixture<SelfRegistrantEndToEndScenarioForMakingARegistrationFreeOfChargeForAConferenceSiteFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "SelfRegistrationFreeOfCharge.feature"
#line hidden
        
        public SelfRegistrantEndToEndScenarioForMakingARegistrationFreeOfChargeForAConferenceSiteFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Self Registrant end to end scenario for making a Registration free of charge for " +
                    "a Conference site", "In order to register for a conference\r\nAs an Attendee\r\nI want to be able to regis" +
                    "ter for the conference free of charge and associate myself with the paid Order a" +
                    "utomatically", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 19
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "seat type",
                        "rate",
                        "quota"});
            table1.AddRow(new string[] {
                        "General admission",
                        "$0",
                        "10"});
            table1.AddRow(new string[] {
                        "Additional cocktail party",
                        "$100",
                        "10"});
#line 20
 testRunner.Given("the list of the available Order Items for the CQRS summit 2012 conference", ((string)(null)), table1);
#line hidden
        }
        
        public virtual void SetFixture(SelfRegistrantEndToEndScenarioForMakingARegistrationFreeOfChargeForAConferenceSiteFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute()]
        [Xunit.TraitAttribute("FeatureTitle", "Self Registrant end to end scenario for making a Registration free of charge for " +
            "a Conference site")]
        [Xunit.TraitAttribute("Description", "Checkout all free of charge")]
        public virtual void CheckoutAllFreeOfCharge()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Checkout all free of charge", ((string[])(null)));
#line 25
this.ScenarioSetup(scenarioInfo);
#line 19
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "seat type",
                        "quantity"});
            table2.AddRow(new string[] {
                        "General admission",
                        "1"});
#line 26
 testRunner.Given("the selected Order Items", ((string)(null)), table2);
#line 29
 testRunner.And("the Registrant proceeds to make the Reservation");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "first name",
                        "last name",
                        "email address"});
            table3.AddRow(new string[] {
                        "William",
                        "Flash",
                        "william@fabrikam.com"});
#line 30
 testRunner.And("the Registrant enters these details", ((string)(null)), table3);
#line 33
 testRunner.And("the total should read $0");
#line 34
 testRunner.When("the Registrant proceeds to Checkout:NoPayment");
#line 35
    testRunner.Then("the Registration process was successful");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "seat type",
                        "quantity"});
            table4.AddRow(new string[] {
                        "General admission",
                        "1"});
#line 36
 testRunner.And("the Order should be created with the following Order Items", ((string)(null)), table4);
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute()]
        [Xunit.TraitAttribute("FeatureTitle", "Self Registrant end to end scenario for making a Registration free of charge for " +
            "a Conference site")]
        [Xunit.TraitAttribute("Description", "Checkout partial free of charge")]
        public virtual void CheckoutPartialFreeOfCharge()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Checkout partial free of charge", ((string[])(null)));
#line 41
this.ScenarioSetup(scenarioInfo);
#line 19
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "seat type",
                        "quantity"});
            table5.AddRow(new string[] {
                        "General admission",
                        "2"});
            table5.AddRow(new string[] {
                        "Additional cocktail party",
                        "3"});
#line 42
 testRunner.Given("the selected Order Items", ((string)(null)), table5);
#line 46
 testRunner.And("the Registrant proceeds to make the Reservation");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "first name",
                        "last name",
                        "email address"});
            table6.AddRow(new string[] {
                        "William",
                        "Flash",
                        "william@fabrikam.com"});
#line 47
 testRunner.And("the Registrant enters these details", ((string)(null)), table6);
#line 50
 testRunner.And("the total should read $300");
#line 51
 testRunner.And("the Registrant proceeds to Checkout:Payment");
#line 52
 testRunner.When("the Registrant proceeds to confirm the payment");
#line 53
    testRunner.Then("the Registration process was successful");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "seat type",
                        "quantity"});
            table7.AddRow(new string[] {
                        "General admission",
                        "2"});
            table7.AddRow(new string[] {
                        "Additional cocktail party",
                        "3"});
#line 54
 testRunner.And("the Order should be created with the following Order Items", ((string)(null)), table7);
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                SelfRegistrantEndToEndScenarioForMakingARegistrationFreeOfChargeForAConferenceSiteFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                SelfRegistrantEndToEndScenarioForMakingARegistrationFreeOfChargeForAConferenceSiteFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
