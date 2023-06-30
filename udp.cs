using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using System.Xml;

public class udp : MonoBehaviour
{
    [SerializeField] string IP;
    Socket server;
    UdpClient udpClient;
    IPEndPoint remoteIPEndPoint;
    MemoryStream memoryStream = new MemoryStream();
    XmlWriterSettings settings = new XmlWriterSettings();

    void Start()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        settings.Encoding = Encoding.UTF8;
        XmlWriter writer = XmlWriter.Create(memoryStream, settings);
        writer.ToString();        

        IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
        int port = 5100;
        remoteIPEndPoint = new IPEndPoint(iPAddress, port);

        server.Connect(remoteIPEndPoint);
        Debug.Log("Conectou");

        QualNacionalidade("Filippo Lippi");
    }

    private void OnDisable()
    {
        server.Close();
    }

    public XmlDocument FormataXML(XmlDocument document)
    {
        StringBuilder output = new StringBuilder();
        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ", 
            NewLineChars = "\r\n", 
            NewLineHandling = NewLineHandling.Replace
        };

        using (XmlWriter writer = XmlWriter.Create(output, settings))
        {
            document.Save(writer);
        }

        string xmlFormatado = output.ToString();
        document.LoadXml(xmlFormatado);
        return document;
    }

    XmlDocument Chamada(XDocument document)
    {

        string xmlString = document.ToString();
        byte[] xmlAutor = Encoding.UTF8.GetBytes(xmlString);
        server.Send(xmlAutor);

        byte[] res = new byte[1024];
        int bytesRead = server.Receive(res);

        string texto = Encoding.UTF8.GetString(res, 0, bytesRead);
        Debug.Log(texto);

        XmlDocument doc = new XmlDocument();
        doc.RemoveAll(); 
        doc.LoadXml(texto);
        doc = FormataXML(doc);
        return doc;
    }

    public void QualNacionalidade(string artista)
    {
        XDocument document = new XDocument(
        new XDeclaration("1.0", "utf-8", "yes"),
        new XElement("inferences",
             new XElement("inference",
                new XAttribute("name", "nationality"),
                new XElement("param",
                    new XAttribute("name", "A"),
                    new XAttribute("type", "var")),
                new XElement("param",
                    new XAttribute("name", artista),
                    new XAttribute("type", "literal"))
                )
            )
        );
        XmlDocument doc = Chamada(document);

        string item = "";
        foreach (XmlElement element in doc.DocumentElement.SelectSingleNode("//inference[@name='nationality']"))
        {
            if(item != element.InnerText)
                item = element.InnerText;
        }
        Debug.Log(item);
    }
}