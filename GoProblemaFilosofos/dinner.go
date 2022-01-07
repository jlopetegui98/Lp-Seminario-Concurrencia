package main

import (
	"fmt"
	"math/rand"
	"time"
)

//estructura para representar a los filosofos
type Philosopher struct {
	name                string
	leftfork, rightfork chan bool
}

//arreglo de 8 tenedores, cada tenedor es un canal que recibe un valor bool
var forks = [8]chan bool{}

//arreglo de 8 filosofos
var philosophers = [8]Philosopher{}

func (phil *Philosopher) sayHi() {
	fmt.Printf("%s se sienta a la mesa y saluda al resto...\n", phil.name)
}

//funcion correspondiente a cuano un filosofo piensa, se detiene la ejecucion de la rutina por un tiempo entre 1 y 10 seg
func (phil *Philosopher) think() {
	fmt.Printf("%s está pensando...\n", phil.name)
	time.Sleep(time.Duration(1+rand.Int63n(10)) * time.Second)
}

//funcion correspondiente a cuando un filosofo come, se detiene la rutina por un tiempo entre 1 y 10 seg
func (phil *Philosopher) eat() {
	fmt.Printf("%s está comiendo...\n", phil.name)
	time.Sleep(time.Duration(1+rand.Int63n(10)) * time.Second)
}

//el filosofo phil suelta sus tenedores
func (phil *Philosopher) dropForks() {
	fmt.Printf("%s ha terminado de comer y suelta sus dos tenedores...\n", phil.name)
	time.Sleep(time.Second)
	phil.leftfork <- true  //envia el valor true por el canal correspondiente al tenedor izquierdo
	phil.rightfork <- true //envia el valor true por el canal correspondiente al tenedor derecho
}

//funcion que representa el momento en que un tenedor trata de coger los tenedores
func (phil *Philosopher) getForks() {
	fmt.Printf("%s está intentando coger los tenedores para comer...\n", phil.name)
	//trata de coger el tenedor izquierdo
	//se crea una subrutina que espera durante 1 segundo y envia por el canal timeout el valor true al finalizar
	timeout := make(chan bool, 1)
	go func() { time.Sleep(1e9); timeout <- true }()
	//este bloque se ejecuta para tratar de leer un valor por el canal leftfork, si llega primero un valor por timeout se llama
	//nuevamente el metodo think y luego recursivamente vuelve a tratar de acceder a los tenedores
	select {
	case <-phil.leftfork:
		fmt.Printf("%s cogió el tenedor a su izquierda...\n", phil.name)
		break
	case <-timeout:
		fmt.Printf("%s se aburrió de esperar por los tenedores, va a pensar un poco...\n", phil.name)
		phil.think()
		phil.getForks()
		return
	}
	//analogo al caso anterior intenta coger el tenedor derecho, si no lo logra libera el izquierdo enviando true por el canal
	//leftfork
	timeoutR := make(chan bool, 1)
	go func() {
		time.Sleep(1e9)
		timeout <- true
	}()

	select {
	case <-phil.rightfork:
		fmt.Printf("%s ya alcanzó los dos tenedores y comenzará a comer...\n", phil.name)
		return
	case <-timeoutR:
		fmt.Printf("%s se aburrió de esperar por el tenedor de la derecha, va a pensar un poco...\n", phil.name)
		phil.leftfork <- true
		phil.think()
		phil.getForks()
		return
	}

}

//rutina correspondiente a cada filosofo
func (phil *Philosopher) goDine(end chan string, n int) {
	for i := 1; i < n; i++ {
		phil.sayHi()
		phil.think()
		phil.getForks()
		phil.eat()
		phil.dropForks()
	}
	end <- phil.name
}

func main() {
	names := [8]string{"Filo1",
		"Filo2",
		"Filo3",
		"Filo4",
		"Filo5",
		"Filo6",
		"Filo7",
		"Filo8"}
	time_init := time.Now()
	//se inicializa cada tenedor
	for i := range names {
		fork := make(chan bool, 1)
		fork <- true
		forks[i] = fork
	}
	//se inicializan los filosofos
	for i, name := range names {
		_phil := &Philosopher{name, forks[i], forks[(i+1)%8]}
		philosophers[i] = *_phil
	}
	//el canal end se utilizara para notificar al final de la ejecucion de la subrutina de cada filosofo a la rutina principal
	//que filosofo termino de comer la cantida de veces que se defina n la variable n
	end := make(chan string, 1)
	for i := range names {
		go philosophers[i].goDine(end, 3) //se ejecuta la subrutina de cada filosofo
	}
	//este ciclo se utiliza para esperar el final de cada go routine que se ejecuto, que se captura a traves del canal end, por
	//el que se pasa el nombre del filosofo de la rutina correspondiente
	for i := range names {
		name := <-end
		fmt.Printf("%s fue el filósofo número %d en terminar de ejecutar su rutina...\n", name, i+1)
	}
	fmt.Printf("Se acabó la cena luego de %f segundos", time.Since(time_init).Seconds())
}
