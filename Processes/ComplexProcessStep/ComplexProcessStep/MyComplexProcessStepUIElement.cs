using System.ComponentModel;
using Neuron.Esb;
using Neuron.Esb.Pipelines;
using Neuron.Pipelines;
using Neuron.Pipelines.Design;
using System.Windows.Forms;

namespace ComplexProcessStep
{
    /// <summary>
    /// This is the UI Element class that determines how the custom Process step will be represented in both the property grid and Process Designer.
    /// Except for the Process Step type (class) name and properties within the Wrapper class, nothing else needs to be changed in the UI Element class.
    /// </summary>
    public class MyComplexProcessStepUIElement : StepUIElement<MyComplexProcessStep>
    {
        public MyComplexProcessStepUIElement(Pipeline<ESBMessage> pipeline)
            : base(pipeline, Globals.StepBitMap)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawLabel(e.Graphics, TypedData.Name);
        }

        public override string Label
        {
            get { return Globals.StepName; }
        }

        public override object BindTo
        {
            get
            {
                return new Wrapper(this.TypedData);
            }
        }

        /// <summary>
        /// This is the wrapper class around the main custom Process Step class.  It exposes the properties bound to the property grid
        /// These properties should map back to the ones in the custom Process Step class. This is where the Properties Type Converter can be used
        /// to control things like ordering of properties, as well as making properties dynamically visible/invisible at design time based on  
        /// input of user
        /// </summary>
        [TypeConverter(typeof(PropertiesTypeConverter))]
        private class Wrapper : BaseWrapper
        {
            private readonly MyComplexProcessStep step;

            public Wrapper(MyComplexProcessStep step) : base(step)
            {
                this.step = step;
            }

            [PropertyOrder(3)]
            [DisplayName("Description")]
            [Description("This is a description of what the step will do.")]
            public string Description
            {
                get
                {
                    return this.step.Description;
                }

                set
                {
                    this.step.Description = value;
                }
            }

            public void DynamicShowMeAttributesProvider(PropertyAttributes attributes)
            {
                attributes.IsBrowsable = (IsEnabled == true);
            }


            [Category("Default")]
            [DisplayName("Message")]
            [PropertyOrder(2)]
            [Description("Test Desc.")]
            public string CustomMessage
            {
                get { return this.step.CustomMessage; }
                set { this.step.CustomMessage = value; }
            }

            [PropertyOrder(1)]
            [Category("Default")]
            [DisplayName("IsEnabled")]
            [Description("To test the functionality of PropertyAttributesProvider.")]
            public bool IsEnabled
            {
                get { return this.step.IsEnabled; }
                set { this.step.IsEnabled = value; }
            }

            [Category("Default")]
            [DisplayName("ShowMe")]
            [PropertyOrder(0)]
            [Description("Should be visible only when IsEnabled is set to true.")]
            [PropertyAttributesProvider("DynamicShowMeAttributesProvider")]
            public string ShowMe
            {
                get { return this.step.ShowMe; }
                set { this.step.ShowMe = value; }
            }
        }
    }
}
