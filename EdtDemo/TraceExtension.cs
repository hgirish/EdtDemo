using System;
using System.IO;
using System.Web.Services.Protocols;

namespace EdtDemo
{ // Define a SOAP Extension that traces the SOAP request and SOAP
// response for the XML Web service method the SOAP extension is
// applied to.

    public class TraceExtension : SoapExtension
    {
        Stream _oldStream;
        Stream _newStream;
        string _filename;

        // Save the Stream representing the SOAP request or SOAP response into
        // a local memory buffer.
        public override Stream ChainStream(Stream stream)
        {
            _oldStream = stream;
            _newStream = new MemoryStream();
            return _newStream;
        }

        // When the SOAP extension is accessed for the first time, the XML Web
        // service method it is applied to is accessed to store the file
        // name passed in, using the corresponding SoapExtensionAttribute.	
        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
        {
            return ((TraceExtensionAttribute)attribute).Filename;
        }

        // The SOAP extension was configured to run using a configuration file
        // instead of an attribute applied to a specific XML Web service
        // method.
        public override object GetInitializer(Type webServiceType)
        {
            // Return a file name to log the trace information to, based on the
            // type.
            return "H:\\Tmp\\" + webServiceType.FullName + ".log";
        }

        // Receive the file name stored by GetInitializer and store it in a
        // member variable for this specific instance.
        public override void Initialize(object initializer)
        {
            _filename = (string)initializer;
        }

        //  If the SoapMessageStage is such that the SoapRequest or
        //  SoapResponse is still in the SOAP format to be sent or received,
        //  save it out to a file.
        public override void ProcessMessage(SoapMessage message)
        {
            switch (message.Stage)
            {
                case SoapMessageStage.BeforeSerialize:
                    break;
                case SoapMessageStage.AfterSerialize:
                    WriteOutput(message);
                    break;
                case SoapMessageStage.BeforeDeserialize:
                    WriteInput(message);
                    break;
                case SoapMessageStage.AfterDeserialize:
                    break;
            }
        }

        public void WriteOutput(SoapMessage message)
        {
            _newStream.Position = 0;
            FileStream fs = new FileStream(_filename, FileMode.Append,
                FileAccess.Write);
            StreamWriter w = new StreamWriter(fs);

            string soapString = (message is SoapServerMessage) ? "SoapResponse" : "SoapRequest";
            w.WriteLine("-----" + soapString + " at " + DateTime.Now);
            w.Flush();
            Copy(_newStream, fs);
            w.Close();
            _newStream.Position = 0;
            Copy(_newStream, _oldStream);
        }

        public void WriteInput(SoapMessage message)
        {
            Copy(_oldStream, _newStream);
            FileStream fs = new FileStream(_filename, FileMode.Append,
                FileAccess.Write);
            StreamWriter w = new StreamWriter(fs);

            string soapString = (message is SoapServerMessage) ?
                "SoapRequest" : "SoapResponse";
            w.WriteLine("-----" + soapString +
                        " at " + DateTime.Now);
            w.Flush();
            _newStream.Position = 0;
            Copy(_newStream, fs);
            w.Close();
            _newStream.Position = 0;
        }

        void Copy(Stream from, Stream to)
        {
            TextReader reader = new StreamReader(from);
            TextWriter writer = new StreamWriter(to);
            writer.WriteLine(reader.ReadToEnd());
            writer.Flush();
        }
    }

// Create a SoapExtensionAttribute for the SOAP Extension that can be
// applied to an XML Web service method.
    [AttributeUsage(AttributeTargets.Method)]
    public class TraceExtensionAttribute : SoapExtensionAttribute
    {

        private string _filename = "H:\\tmp\\log.txt";
        private int _priority;

        public override Type ExtensionType
        {
            get { return typeof(TraceExtension); }
        }

        public override int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
            }
        }
    }
}