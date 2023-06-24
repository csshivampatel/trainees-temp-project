using System.ComponentModel;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using Neuron.Esb;
using Neuron.Esb.Pipelines;
using Neuron.Pipelines;
using Neuron.Pipelines.Design;

namespace ComplexProcessStep
{
    public static class Globals
    {
        /// <summary>
        /// Used by both the Process Step class and the UI element class to control the custom name of the Process step that appears in the Process Designer
        /// </summary>
        public const string StepName = "My Complex Process Step";
        /// <summary>
        /// Used by the UI Element class to control what Image is used to represent the custom process step within the "Process Steps" UI panel to the right of the Process Designer of the Neuron ESB Explorer
        /// </summary>
        public static System.Drawing.Bitmap StepBitMap { get {return Resource1.ComplexProcessStep_MyComplexProcessStep;} }
    }
 
    /// <summary>
    /// The Display name is the name listed under the "Process Steps" UI panel to the right of the Process Designer of the Neuron ESB Explorer
    /// The Description is the mouse over text that pops up when the process step is selected in the "Process Steps" UI panel
    /// The icon that is displayed to represent the process step in the "Process Steps" UI is determined in the constructor of the UI Element class that uses the PNG image
    /// located in the Resources directory.
    /// 
    /// ***If the same image is to be used for displaying the custom Process Step in the main designer (i.e. dragging it onto a Process), then ensure the image name in the Resource Folder is in the
    /// form of 'namespace'.'class'.png, and then copy that image to the Neuron Pipelines directory along with the custom process step assembly. 
    /// </summary>
    [DataContract]
    [Description("Sample process step with a custom UI")]
    [DisplayName(Globals.StepName)]
    [ProcessStep(typeof(MyComplexProcessStep), typeof(Resource1), "name", "description", "ComplexProcessStep_MyComplexProcessStep", typeof(MyComplexProcessStepUIElement),
    Path = "Custom Process Steps")]
    public class MyComplexProcessStep : CustomPipelineStep
    {
        
        /// <summary>
        /// If these properties are required for design time configuration then they must be replicated in the Wrapper class of the UI Element class
        /// </summary>
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string CustomMessage { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public string ShowMe { get; set; }

        /// <summary>
        /// Set this.Name to provide a default name to the process step when it's dragged onto the Process Designer.
        /// This only fires when a user drags the step onto the Process Designer at design time
        /// </summary>
        public MyComplexProcessStep()
        {
            this.Name = "My Complex Process Step";
        }

        /// <summary>
        /// This is where the magic should happen....all code to manipulate change or process the incoming ESB Message
        /// </summary>
        /// <param name="context"></param>
        protected override void OnExecute(PipelineContext<ESBMessage> context)
        {
            System.IO.File.WriteAllText(@"C:\complexprocessstep.txt", ("MyComplexProcessStep called. Value of MyProperty1 = " + CustomMessage));
            context.Instance.TraceInformation("MyCustomProcessStep called. Value of MyProperty1 = " + CustomMessage);
        }
    }
}
