using System;
using System.Collections.Generic;
using Neuron.Esb;
using Neuron.Esb.Pipelines;
using Neuron.Pipelines;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Resources;
using System.Transactions;

namespace SimpleProcessStep
{
    [DisplayName("My Simple Process Step")]
    [ProcessStep(typeof(MySimpleProcessStep), typeof(Resource1), "name", "description", "Image1","",
    Path = "Custom Process Steps")]
    public class MySimpleProcessStep : CustomPipelineStep
    {
        [DataMember]
        [Category("My Category")]
        [DisplayName("My Property1")]
        [Description("My Property1 Description")]
        [Bindable(false)]
        public String MyProperty1 { get; set; }

        [DataMember]
        [Category("My Category")]
        [DisplayName("My Property2")]
        [Description("My Property2 Description")]
        [Bindable(false)]
        public String MyProperty2 { get; set; }

        [Bindable(false)]
        [Browsable(false)]
        public String MyProperty3 { get; set; }

        public String MyProperty4 { get; set; }

        String MyPrivateProperty { get; set; }
        
        int _myProperty5 = 3;
        [DefaultValue(3)]
        public int MyProperty5 { get { return _myProperty5; } set { _myProperty5 = value; } }

        TransactionScopeOption _myProperty6 = TransactionScopeOption.Suppress;
        [DefaultValue(TransactionScopeOption.Suppress)]
        public TransactionScopeOption MyProperty6 { get { return _myProperty6; } set { _myProperty6 = value; } }

        String _myProperty7 = "MyProperty7";
        [DefaultValue("MyProperty7")]
        public String MyProperty7 { get { return _myProperty7; } set { _myProperty7 = value; } }


        [TypeConverter(typeof(MyProperty8TypeConverter))]
        public String MyProperty8 { get; set; }

        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public String MyProperty9 { get; set; }

        /// <summary>
        /// Set this.Name to provide a default name to the process step when it's dragged onto the Process Designer.
        /// This only fires when a user drags the step onto the Process Designer at design time
        /// </summary>
        public MySimpleProcessStep()
        {
            this.Name = "My Simple Process Step";
        }

        protected override void OnExecute(PipelineContext<ESBMessage> context)
        {
            System.IO.File.WriteAllText(@"C:\simpleprocessstep.txt", ("MySimpleProcessStep called. Value of MyProperty1 = " + MyProperty1));
            context.Instance.TraceInformation("MyCustomProcessStep called. Value of MyProperty1 = " + MyProperty1);
            
        }
    }


    public class MyProperty8TypeConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<String> values = new List<string>();
            values.Add("Value1");
            return new StandardValuesCollection(values.ToArray());
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }
                   
    }

}
