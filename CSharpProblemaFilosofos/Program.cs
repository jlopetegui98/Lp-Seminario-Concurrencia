using System;
using System.Collections.Generic;
using System.Threading;

namespace dinner{

    class Program{
        static Random r = new Random();
        static object mesero=new object();
        class Tenedor{}
        class Filosofo{
            Tenedor izq,der;
            string nombre;
            public Filosofo(Tenedor izq, Tenedor der, string nombre){
                this.izq=izq;
                this.der=der;
                this.nombre=nombre;
            }
            public void Vive(){
                int n = 3;
                while(n > 0){

                    Console.WriteLine("{0, -30}{1}","Pensando....",nombre);
                    Thread.Sleep(1+r.Next(10000));
                    Console.WriteLine("{0,-30}{1}","Queriendo comer", nombre); 
                    Monitor.Enter(mesero);

                    // Empieza la seccion critica de este proceso
                    lock(izq){
                        // Demora en tomar un tenedor
                        Thread.Sleep(1000);

                        lock(der){

                            Monitor.Exit(mesero);
                            Console.WriteLine("{0, -30}{1}","Comiendo....",nombre);
                            Thread.Sleep(r.Next(1000,10000)); 
                        }
                    }
                    // Termina la seccion critica del proceso
                Console.WriteLine("{0, -30}{1}","Regreso a Pensar....",nombre);
                n--;}
            }
        }
        public static void Main (string [] args){
                Tenedor[] tenedores = new Tenedor[8] {       
                                new Tenedor(), new Tenedor(),         
                                new Tenedor(), new Tenedor(),  new Tenedor() , new Tenedor(),
                                new Tenedor(),new Tenedor()};   
            
                 Filosofo socrates = new Filosofo(tenedores[0],tenedores[1], "Socrates");
                 Filosofo platon = new Filosofo(tenedores[1],tenedores[2], "Platon"); 
                 Filosofo seneca = new Filosofo(tenedores[2],tenedores[3], "Séneca"); 
                 Filosofo diogenes = new Filosofo(tenedores[3],tenedores[4], "Diógenes"); 
                 Filosofo aristoteles = new Filosofo(tenedores[4],tenedores[5],"Aristóteles"); 
                 Filosofo sun_tzu = new Filosofo(tenedores[5],tenedores[6],"Sun Tzu");
                 Filosofo Voltair = new Filosofo(tenedores[6],tenedores[7],"Voltaire");
                 Filosofo Descarte = new Filosofo(tenedores[7],tenedores[0],"Descarte");

                 new Thread(socrates.Vive).Start();   
                 new Thread(platon.Vive).Start();   
                 new Thread(seneca.Vive).Start();   
                 new Thread(diogenes.Vive).Start();   
                 new Thread(aristoteles.Vive).Start();
                 new Thread(sun_tzu.Vive).Start(); 
                 new Thread(Voltair.Vive).Start();  
                 new Thread(Descarte.Vive).Start(); 


            
        }
    }
}