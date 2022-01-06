package main

import (
	"fmt"
	"math/rand"
	"sync"
	"time"
)

var wg sync.WaitGroup

type Philosopher struct {
	name                string
	leftfork, rightfork chan bool
}

var forks = [8]chan bool{}
var philosophers = [8]Philosopher{}

func (phil *Philosopher) sayHi() {
	fmt.Printf("%s se sienta a la mesa y saluda al resto...\n", phil.name)
}

func (phil *Philosopher) think() {
	fmt.Printf("%s está pensando...\n", phil.name)
	time.Sleep(time.Duration(1+rand.Int63n(10)) * time.Second)
}

func (phil *Philosopher) eat() {
	fmt.Printf("%s está comiendo...\n", phil.name)
	time.Sleep(time.Duration(1+rand.Int63n(10)) * time.Second)
}

func (phil *Philosopher) dropForks() {
	fmt.Printf("%s ha terminado de comer y suelta sus dos tenedores...\n", phil.name)
	time.Sleep(time.Second)
	phil.leftfork <- true
	phil.rightfork <- true
}

func (phil *Philosopher) getForks() {
	fmt.Printf("%s está intentando coger los tenedores para comer...\n", phil.name)
	timeout := make(chan bool, 1)
	go func() { time.Sleep(1e9); timeout <- true }()

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

	timeoutR := make(chan bool, 1)
	go func() { time.Sleep(1e9); timeoutR <- true }()

	select {
	case <-phil.rightfork:
		fmt.Printf("%s ya alcanzó los dos tenedores y comenzará a comer...\n", phil.name)
		return
	case <-timeoutR:
		fmt.Printf("%s se aburrió de esperar por el tenedor de la derecha, va a pensar un poco...\n", phil.name)
		phil.leftfork <- true
		phil.think()
		phil.getForks()
	}

}

func (phil *Philosopher) goDine() {
	phil.sayHi()
	phil.think()
	fmt.Println("Dejo de pensar")
	phil.getForks()
	phil.eat()
	phil.dropForks()
	defer wg.Done()
}

func main() {
	names := [8]string{"Aristotle",
		"Immanuel Kant",
		"Confucius",
		"Rene Descarte",
		"John Locke",
		"Voltaire",
		"Baron de Montesquieu",
		"Sun Tzu"}

	for i := range names {
		fork := make(chan bool, 1)
		fork <- true
		forks[i] = fork
	}
	for i, name := range names {
		_phil := &Philosopher{name, forks[i], forks[(i+1)%8]}
		philosophers[i] = *_phil
	}
	for i := range names {
		go philosophers[i].goDine()
		wg.Add(1)
	}
	wg.Wait()
	fmt.Println("Se acabó la cena...")
}
