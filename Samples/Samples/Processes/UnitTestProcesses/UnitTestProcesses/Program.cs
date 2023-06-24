using System;
using System.IO;
using System.Xml.Schema;
using Neuron.Esb.Pipelines;
using Neuron.Esb.Administration;
using Neuron.Pipelines;
using System.Net;
using Neuron.Esb.Internal;
using Neuron.Esb;

namespace UnitTestProcessSteps
{
    class Program
    {
        #region Constants
        private const string _CORRELATIONID = @"220b1f11-23b6-460c-a83b-70907e9dff45\3940494";
        private const string _ESBSOLUTION_FILE = @"\..\..\..\..\..\Configurations\MessageValidationTransformation";
        private const string _SERVICEIDENTITY = "";
        private const string _BOOTSTRAP_ADDR = "net.tcp://localhost:50000";
        private static NetworkCredential _clientCredentials = System.Net.CredentialCache.DefaultNetworkCredentials;
        private const string _VALIDATION_SCHEMA = 
@"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='book'>
    <xs:complexType>
      <xs:sequence>
        <xs:element name='title' type='xs:string' />
        <xs:element name='author' type='xs:string' />
        <xs:element name='character' minOccurs='0' maxOccurs='unbounded'>
          <xs:complexType>
            <xs:sequence>
              <xs:element name='name' type='xs:string' />
              <xs:element name='friend-of' type='xs:string' minOccurs='0' maxOccurs='unbounded' />
              <xs:element name='since' type='xs:date' />
              <xs:element name='qualification' type='xs:string' />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
      <xs:attribute name='isbn' type='xs:string' />
    </xs:complexType>
  </xs:element>
</xs:schema>";
        private const string _TRANSFORM_XSLT = 
@"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
<xsl:template match='/'>
  <html>
  <body>
	<h2><xsl:value-of select='./title' /></h2>
	<h3><xsl:value-of select='./author' /></h3>
    <table border='1'>
    <tr bgcolor='#9acd32'>
      <th align='left'>Name</th>
      <th align='left'>Since</th>
	  <th align='left'>Qualification</th>
    </tr>
    <xsl:for-each select='character'>
    <tr>
      <td><xsl:value-of select='./name' /></td>
      <td><xsl:value-of select='./since' /></td>
	  <td><xsl:value-of select='./qualification' /></td>
    </tr>
    </xsl:for-each>
    </table>
  </body>
  </html>
</xsl:template>
</xsl:stylesheet>";
        private const string _TEST_XML = 
@"<book isbn='0836217462'>
    <title>Being a Dog Is a Full-Time Job</title> 
    <author>Charles M. Schulz</author> 
	<character>
		<name>Snoopy</name> 
		<friend-of>Peppermint Patty</friend-of> 
		<since>1950-10-04</since> 
		<qualification>extroverted beagle</qualification> 
  	</character>
	<character>
		<name>Peppermint Patty</name> 
		<since>1966-08-22</since> 
		<qualification>bold, brash and tomboyish</qualification> 
	</character>
</book>";
        #endregion

        // TestValidateSchemaStep and TestTransformXsltStep will work without any addition setup.
        // TestMsmqStepSend and TestMsmqStep Receive require a local transactional private queue called unit_test_msmq.
        //      The user running the unit test needs to have full access to the queue.
        //      References MUST be added to Neuron.dll, Neuron.Esb.dll and Neuron.Pipelines.dll
        static void Main(string[] args)
        {
            TestValidateSchemaStep();
            TestTransformXsltStep();
            //TestMsmqStepSend();
            //TestMsmqStepReceive();
            ExecuteProcess();
            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        static void TestValidateSchemaStep()
        {
            Console.WriteLine("Test Validate-Schema Step");

            // create the esb message
            ESBMessage outMsg = null;
            ESBMessage msg = new ESBMessage();

            // Set the message-specific properties
            msg.Header.Topic = "Books";
            msg.Header.Semantic = Semantic.Multicast;
            msg.Header.Action = "";

            // To test a failed validation, comment the next line and uncomment the line after
            msg.FromXml(_TEST_XML);
            //msg.FromXml("<test>my message</test>");

            PipelineRuntime runtime = new PipelineRuntime { DesignMode = true };
            Pipeline<ESBMessage> process = new Pipeline<ESBMessage>();

            // Create the Validate-Schema step
            EsbMessageSchemaValidationPipelineStep processStep = new EsbMessageSchemaValidationPipelineStep();
            EsbMessageSchemaValidationPipelineStep.DesignModeEsbConfigSelector = new EsbConfigSelectorDelegate(() => { return new ESBConfiguration(); });

            // Set the step-specific properties
            processStep.Schemas.Add(XmlSchema.Read(new StringReader(_VALIDATION_SCHEMA), null));
            processStep.Name = "My Custom Validation Step";

            // Remove the default Cancel pipeline step that is part of the Invalid branch
            processStep.OnInvalid.Children.RemoveAt(0);

            // Add a rethrow step in the Invalid branch to throw any exception
            processStep.OnInvalid.Children.Add(new RethrowPipelineStep<ESBMessage>());

            // Add the Validate-Schema step to the process
            process.Steps.Add(processStep);

            // Create the process instance
            PipelineInstance<ESBMessage> instance = runtime.CreateInstance(process);

            try
            {
                // Execute the process
                outMsg = instance.Execute(msg);

                // Exceptions are handled by the processes and set in the Instance.UnhandledException property
                if (instance.UnhandledException == null)
                    Console.WriteLine("Message Validation Succeeded");
                else
                    Console.WriteLine(String.Format("Message Validation Failed: {0}", instance.UnhandledException.Message != null ? instance.UnhandledException.Message : "No exception details provided"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Message Validation Failed: " + ex.Message);
            }

            Console.WriteLine();
        }

        static void TestTransformXsltStep()
        {
            Console.WriteLine("Test Transform-Xslt Step");

            // create the esb message
            ESBMessage outMsg = null;
            ESBMessage msg = new ESBMessage();

            // Set the message-specific properties
            msg.Header.Topic = "Books";
            msg.Header.Semantic = Semantic.Multicast;
            msg.Header.Action = "";

            // To test a failed validation, comment the next line and uncomment the line after
            msg.FromXml(_TEST_XML);
            //msg.FromXml("<test>my message</test>");

            PipelineRuntime runtime = new PipelineRuntime { DesignMode = true };
            Pipeline<ESBMessage> process = new Pipeline<ESBMessage>();

            // Create the Transform-Xslt step
            EsbMessageBodyXslTransformPipelineStep processStep = new EsbMessageBodyXslTransformPipelineStep();
            EsbMessageBodyXslTransformPipelineStep.DesignModeEsbConfigSelector = new EsbConfigSelectorDelegate(() => { return new ESBConfiguration(); });

            // Set the step-specific properties
            processStep.TransformXml = _TRANSFORM_XSLT;
            processStep.Name = "My Custom Transformation Step";

            // Add the Transform-Xslt step to the process
            process.Steps.Add(processStep);

            // Create the process instance
            PipelineInstance<ESBMessage> instance = runtime.CreateInstance(process);

            try
            {
                // Execute the process
                outMsg = instance.Execute(msg);

                // Note - unless there is a catastrophic error, the transform will most likely succeed.  The output will just miss the mapped elements
                if (instance.UnhandledException == null)
                    Console.WriteLine("Message Transformation Succeeded: " + outMsg.Text);
                else
                    Console.WriteLine(String.Format("Message Transformation Failed: {0}", instance.UnhandledException.Message != null ? instance.UnhandledException.Message : "No exception details provided"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Message Transformation Failed: " + ex.Message);
            }

            Console.WriteLine();
        }

        static void TestMsmqStepSend()
        {
            Console.WriteLine("Test Msmq (send) Step");

            // create the esb message
            ESBMessage outMsg = null;
            ESBMessage msg = new ESBMessage();

            // Set the message-specific properties
            msg.Header.Topic = "Services";
            msg.Header.Semantic = Semantic.Multicast;
            msg.Header.Action = "";
            msg.FromXml("<xml>Unit test MSMQ message...</xml>");

            // set the correlation id of the message
            msg.SetProperty("msmq", "CorrelationId", _CORRELATIONID);

            PipelineRuntime runtime = new PipelineRuntime { DesignMode = true };
            Pipeline<ESBMessage> process = new Pipeline<ESBMessage>();

            // Create the Msmq step
            EsbMessageMsmqPipelineStep step = new EsbMessageMsmqPipelineStep();

            // Set the step-specific properties
            step.Direction = EsbMessageMsmqPipelineStep.DirectionEnum.Send;
            step.Name = "My Custom Queue Step";
            step.Transactional = EsbMessageMsmqPipelineStep.TransactionEnum.Required;
            step.UseActiveXMessageFormatter = true;
            step.QueuePath = @".\private$\unit_test_msmq";
            step.ClearConnectionCache = false;

            // These properties are specific for Sending messages with the Msmq step
            step.Label = "Unit test label";
            step.AppSpecific = 8;
            step.Priority = System.Messaging.MessagePriority.VeryHigh;

            // Add the Msmq step to the process
            process.Steps.Add(step);

            // Create the process instance
            PipelineInstance<ESBMessage> instance = runtime.CreateInstance(process);

            // Create the Aborted event handler
            instance.Aborted -= new EventHandler(instance_Aborted);
            instance.Aborted += new EventHandler(instance_Aborted);

            // Execute the process
            outMsg = instance.Execute(msg);

            // Get the correlation ID from the message properties
            string msgId = outMsg.GetProperty("msmq", "CorrelationId");
            Console.WriteLine("Msmq Message Sent: Correlation ID = " + msgId + System.Environment.NewLine + "   " + outMsg.Text);

            Console.WriteLine();
        }

        static void TestMsmqStepReceive()
        {
            Console.WriteLine("Test Msmq (receive) Step");

            // create the esb message
            ESBMessage outMsg = null;
            ESBMessage msg = new ESBMessage();

            PipelineRuntime runtime = new PipelineRuntime { DesignMode = true };
            Pipeline<ESBMessage> process = new Pipeline<ESBMessage>();

            // Create the Msmq step
            EsbMessageMsmqPipelineStep processStep = new EsbMessageMsmqPipelineStep();

            // Set the step-specific properties
            processStep.Direction = EsbMessageMsmqPipelineStep.DirectionEnum.Receive;
            processStep.Name = "My Custom Queue Step";
            processStep.Transactional = EsbMessageMsmqPipelineStep.TransactionEnum.Required;
            processStep.UseActiveXMessageFormatter = true;
            processStep.QueuePath = @".\private$\unit_test_msmq";

            // These properties are specific for Receiving messages with the Msmq step
            processStep.TimeoutReceive = new TimeSpan(0, 2, 0);
            processStep.Correlate = true;

            // Set the value of correlation id to look up in the queue
            msg.SetProperty("msmq", "CorrelationId", _CORRELATIONID);

            // Add the Msmq step to the process
            process.Steps.Add(processStep);

            // Create the process instance
            PipelineInstance<ESBMessage> instance = runtime.CreateInstance(process);

            // Create the Aborted event handler
            instance.Aborted -= new EventHandler(instance_Aborted);
            instance.Aborted += new EventHandler(instance_Aborted);

            // Execute the process
            outMsg = instance.Execute(msg);

            // Get the correlation ID from the message properties
            string msgId = outMsg.GetProperty("msmq", "CorrelationId");
            Console.WriteLine("Msmq Message Received: Correlation ID = " + msgId + System.Environment.NewLine + "   " + outMsg.Text);

            Console.WriteLine();
        }

        /// <summary>
        /// To use this the ESB Configuration (represented by the CONSTANT, _ESBSOLUTION_FILE) must be opened in the Neuron ESB Explorer. 
        /// Then open the "Validate Book Message - Code" Process and disable the "set schema & xslt" Process Step.
        /// Next, replace the Cancel step with a Rethrow step. Save the ESB Configuration.
        /// The first test should work and validate. After the first run, open the Neuron ESB Configuration and change
        /// the BookSchema XSD in the Repository so that it expects a different root element name (i.e. "BookSmart" instead of "Books")
        /// Once that is done attempt to run again. this time it should return the error that the root element name is invalid
        /// </summary>
        static void ExecuteProcess()
        {
            Console.WriteLine("Test full process from solution");

            // Set the names of the schema and transform to be used in the process.  These docs are in the Repository
            string schema = "BookSchema";
            string transform = "BookToHtmlXslt";
            string processName = "Validate Book Message - Code";

            // Create Neuron ESB message and Process Runtime
            PipelineRuntime runtime = new PipelineRuntime { DesignMode = true };
            ESBMessage msg = new ESBMessage();
            ESBMessagePipelineStorage processStorage;

            // Set any message properties that the pipeline may use as well as the message to process
            // set the dynamic properties for schema and transform if need to set externally
            msg.Header.Semantic = Semantic.Multicast;
            msg.Header.Action = "";
            msg.Header.Topic = "Books";
            msg.SetProperty("neuron", "xsltName", transform);
            msg.SetProperty("neuron", "schemaNames", schema);
            msg.FromXml(_TEST_XML);

            // Open an instance of the Neuron ESB Configuration
            using (Administrator admin = AdminOffLine())
            {
                admin.OpenConfiguration();
                
                InitializeProcessStepDesignTimeConfig(admin.Configuration);
                // Retreive the pipeline blob object from the ESB config
                // Create a Pipeline by binding to the ClientPipelineItem object
                processStorage = admin.GetPipelineListItemByName(processName);
                
                admin.CloseConfiguration();
            }

            if (processStorage != null)
            {
                ClientPipelineItem clientProcess = CreateProcessItemFromStorage(processStorage);

                // Create a Pipeline Runtime instance and execute
                var instance = runtime.CreateInstance(clientProcess.ThePipeline);

                // Create the Aborted event handler
                instance.Aborted += new EventHandler(instance_Aborted);

                // Execute the process
                Neuron.Esb.ESBMessage outMsg = instance.Execute(msg);

                // Test the state and retrieve exception
                if (instance.UnhandledException == null && instance.State != PipelineState.Aborted)
                    Console.WriteLine("Execution Succeeded: " + outMsg.Text);
                else
                    Console.WriteLine(String.Format("Execution Failed: {0}", instance.UnhandledException != null ? instance.UnhandledException.Message : "No exception details provided"));

            }
            else
                Console.WriteLine(string.Format("No pipeline by name '{0}' found", processName));

            Console.WriteLine();
        }
        
        static void instance_Aborted(object sender, EventArgs e)
        {
            PipelineInstance<Neuron.Esb.ESBMessage> obj = (PipelineInstance<ESBMessage>)sender;
           
            if(obj.UnhandledException != null && obj.UnhandledException.Message != null)
                Console.WriteLine("Exception: " + obj.UnhandledException.Message);
        }

        #region Esb Config Helpers
        /// <summary>
        /// Retrieves the Neuron ESB Configuration currently loaded by the ESB runtime and returns the Neuron ESB 
        /// Administration object used to execute operations against the running online configuration
        /// </summary>
        /// <returns>Administration object</returns>
        private static Administrator AdminOnline()
        {
            return new Administrator(_BOOTSTRAP_ADDR, _SERVICEIDENTITY, _clientCredentials);
        }
        /// <summary>
        /// Opens the Neuron ESB Configuration stored on disk and returns the Neuron ESB Administration object used
        /// to execute operations against the offline configuration
        /// </summary>
        /// <returns>Administration object</returns>
        private static Administrator AdminOffLine()
        {
            return new Administrator(System.Environment.CurrentDirectory + _ESBSOLUTION_FILE);
        }
        /// <summary>
        /// Used to create the ClientPipelineItem that contains the Pipeline<ESBMessage> object (the Business Process) required 
        /// by the Pipeline runtime. This is created by retrieving the xml blob that represents the process 
        /// from the Neuron ESB configuration
        /// </summary>
        /// <param name="processStorage">The ESBMessagePipelineStorage that contains the xml blob representing the Process</param>
        /// <returns></returns>
        private static ClientPipelineItem CreateProcessItemFromStorage(ESBMessagePipelineStorage processStorage)
        {
            ClientPipelineItem clientProcess = new ClientPipelineItem();
            clientProcess.ApplyOnPublish = true;
            clientProcess.Name = processStorage.Name;
            clientProcess.Id = processStorage.Id;
            clientProcess.PipelineBlob = processStorage.PipelineBlob;
            clientProcess.ThePipeline.ApplyBindings();

            if (clientProcess.PipelineConditions == null)
                clientProcess.PipelineConditions = new ESBMessagePattern();

            if (clientProcess.PipelineConditions.Condition == null)
                clientProcess.PipelineConditions.Condition = new Condition();

            return clientProcess;
        }
        /// <summary>
        /// Will initialize the process step design time environment with the current Neuron ESB Configuration.
        /// This is only necessary with some Neuron ESB Process steps that references the configuration in their 
        /// source code
        /// </summary>
        /// <param name="config"></param>
        private static void InitializeProcessStepDesignTimeConfig(ESBConfiguration config)
        {
            // if setting dynamic properties, we have to supply a pointer to the esb config to all the steps that are using
            // dynamic properties or reference the configuration for other work. Once we do that. 
            EsbMessageSchemaValidationPipelineStep.DesignModeEsbConfigSelector = new EsbConfigSelectorDelegate(() => { return config; });
            EsbMessageBodyXslTransformPipelineStep.DesignModeEsbConfigSelector = new EsbConfigSelectorDelegate(() => { return config; });
            EsbMessagePipelineExecutionPipelineStep.DesignModeEsbConfigSelector = new EsbConfigSelectorDelegate(() => { return config; });
            DetectDuplicatesStep.DesignModeEsbConfigSelector = new EsbConfigSelectorDelegate(() => { return config; });
            EsbMessageAuditPipelineStep.DesignModeEsbConfigSelector = new EsbConfigSelectorDelegate(() => { return config; });
            EsbMessagePublishPipelineStep.DesignModeEsbConfigSelector = new EsbConfigSelectorDelegate(() => { return config; });
            EsbMessageServiceEndpointPipelineStep.DesignModeEsbConfigSelector = new EsbConfigSelectorDelegate(() => { return config; });

        }
        #endregion
    }
}
