﻿using SistemaBlueddit.Client.Logic;
using SistemaBlueddit.Domain;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SistemaBlueddit.Client
{
    public class LocalRequestHandler
    {
        private TopicLogic _topicLogic;
        private PostLogic _postLogic;
        private FileLogic _fileLogic;
        private ResponseLogic _responseLogic;
        private bool exit;

        public LocalRequestHandler(TopicLogic topicLogic, PostLogic postLogic, FileLogic fileLogic, ResponseLogic responseLogic)
        {
            _topicLogic = topicLogic;
            _postLogic = postLogic;
            _fileLogic = fileLogic;
            _responseLogic = responseLogic;
        }

        public void HandleLocalRequests()
        {
            Console.WriteLine("Cliente se esta iniciando");

            var tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
            tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 50000);

            Console.WriteLine("Cliente se conectó al servidor.");

            try
            {
                while (!exit)
                {
                    Console.WriteLine("***************************************");
                    Console.WriteLine("*   Bienvenido al Sistema Blueddit    *");
                    Console.WriteLine("***************************************");
                    Console.WriteLine("* 1 - Crear un nuevo tema             *");
                    Console.WriteLine("* 2 - Crear un nuevo post             *");
                    Console.WriteLine("* 3 - Subir archivo a una publicacion *");
                    Console.WriteLine("* 4 - Dar de baja un tema             *");
                    Console.WriteLine("* 5 - Modificar un tema               *");
                    Console.WriteLine("* 6 - Dar de baja un post             *");
                    Console.WriteLine("* 7 - Modificar un post               *");
                    Console.WriteLine("* 99 - salir                          *");
                    Console.WriteLine("***************************************");
                    var option = Console.ReadLine();
                    switch (option)
                    {
                        case "1":
                            var topicToCreate = GetTopicToCreate();
                            _topicLogic.Create(tcpClient, option, topicToCreate);
                            var createTopicResponse = _responseLogic.HandleResponse(tcpClient);
                            Console.WriteLine(createTopicResponse.ServerResponse);
                            break;
                        case "2":
                            var postToCreate = GetPostToCreate();
                            _postLogic.Create(tcpClient, option, postToCreate);
                            var createPostResponse = _responseLogic.HandleResponse(tcpClient);
                            Console.WriteLine(createPostResponse.ServerResponse);
                            break;
                        case "3":
                            Console.WriteLine("Escriba el nombre del la publicacion");
                            var postName = Console.ReadLine();
                            var postToUploadFile = new Post
                            {
                                Name = postName,
                                Topics = new List<Topic>()
                            };
                            _postLogic.Exists(tcpClient, option, postToUploadFile);
                            var existsPostResponse = _responseLogic.HandleResponse(tcpClient);
                            if (existsPostResponse.ServerResponse.Equals("existe"))
                            {
                                Console.WriteLine("Escriba el path completo del archivo a subir");
                                var path = Console.ReadLine();
                                _fileLogic.SendFile(option, path, tcpClient);
                                var fileSendConfirmation = _responseLogic.HandleResponse(tcpClient);
                                Console.WriteLine(fileSendConfirmation.ServerResponse);
                            }
                            else
                            {
                                Console.WriteLine("No existe el nombre de la publicacion ingresada");
                            }
                            break;
                        case "4":
                            Console.WriteLine("Escriba el nombre del tema a dar de baja");
                            var topicName = Console.ReadLine();
                            var topicToDelete = new Topic
                            {
                                Name = topicName
                            };
                            _topicLogic.Delete(tcpClient, option, topicToDelete);
                            var topicDeleteResponse = _responseLogic.HandleResponse(tcpClient);
                            Console.WriteLine(topicDeleteResponse.ServerResponse);
                            break;
                        case "5":
                            var topicToModify = GetTopicToCreate();
                            _topicLogic.Update(tcpClient, option, topicToModify);
                            var topicModifiedResponse = _responseLogic.HandleResponse(tcpClient);
                            Console.WriteLine(topicModifiedResponse.ServerResponse);
                            break;
                        case "6":
                            Console.WriteLine("Escriba el nombre del post a dar de baja");
                            var postNameToDelete = Console.ReadLine();
                            var postToDelete = new Post
                            {
                                Name = postNameToDelete,
                                Topics = new List<Topic>()
                            };
                            _postLogic.Delete(tcpClient, option, postToDelete);
                            var postDeletedResponse = _responseLogic.HandleResponse(tcpClient);
                            Console.WriteLine(postDeletedResponse.ServerResponse);
                            break;
                        case "7":
                            var postToModify = GetPostToCreate();
                            _postLogic.Update(tcpClient, option, postToModify);
                            var postModifiedResponse = _responseLogic.HandleResponse(tcpClient);
                            Console.WriteLine(postModifiedResponse.ServerResponse);
                            break;
                        case "99":
                            exit = true;
                            tcpClient.GetStream().Close();
                            tcpClient.Close();
                            break;
                        default:
                            Console.WriteLine("Opcion invalida...");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ha habido un error: " + e.Message+" "+e.ToString());
                Console.WriteLine("Se perdió la conexión con el servidor.");
            }
        }

        public Post GetPostToCreate()
        {
            var newPost = new Post();
            var exit = false;
            var postTopics = new List<Topic>();
            Console.WriteLine("Nombre del post:");
            var name = Console.ReadLine();
            Console.WriteLine("Agregar temas. Cuando termine de agregar temas ingrese la letra s:");
            while (!exit)
            {
                Console.WriteLine("Ingrese el nombre del tema");
                var topicName = Console.ReadLine();
                if (topicName.Equals("s"))
                {
                    exit = true;
                }
                else
                {
                    if (postTopics.Exists(topic => topic.Name == topicName))
                    {
                        Console.WriteLine("Nombre del tema repetido");
                    }
                    else
                    {
                        var topic = new Topic
                        {
                            Name = topicName
                        };
                        postTopics.Add(topic);
                    }
                }
            };
            newPost.Name = name;
            newPost.Topics = postTopics;
            newPost.CreationDate = DateTime.Now;
            return newPost;
        }

        public Topic GetTopicToCreate()
        {
            Console.WriteLine("Nombre del tema:");
            var name = Console.ReadLine();
            Console.WriteLine("Descripcion:");
            var description = Console.ReadLine();

            var topic = new Topic
            {
                Name = name,
                Description = description
            };
            return topic;
        }
    }
}