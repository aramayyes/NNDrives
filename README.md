# NNDrives - Autonomous Cars and Neuroevolution

## Bachelor thesis

This repository contains the source code of my bachelor thesis project.

- University - [Yerevan State University](http://www.ysu.am/main/en)
- Faculty - [Faculty of Informatics and Applied Mathematics](http://www.ysu.am/faculties/en/Informatics-and-Applied-Mathematics)
- Specialty - "Mathematics. Computer science"

Here are the links to get the **PDF** version of my bachelor thesis titled "Modelling and Simulation of Autonomous Cars in Bi-Dimensional Space":
- [in Armenian](https://www.dropbox.com/s/9zjcol1u11brad7/BachelorThesis.pdf?dl=0)
- [in Russian](https://www.dropbox.com/s/vxy9eix74meyvgc/BachelorThesis%20-%20Modelling%20and%20Simulation%20of%20Autonomous%20Cars%20%28RU%29.pdf?dl=0)   

## Project

For convenience, the project has its own title: "NNDrives - Autonomous Cars and Neuroevolution".

**NNDrives** is a [Unity](https://unity3d.com) project, that allows to create and test autonomous cars. Simulation environment is the bi-dimensional space, that one can see being in a helicopter.

Cars development is done using **neural networks** and **genetic algorithm**.

## Simulation

Car is equipped with sensors, each of which measures the distance to the nearest object in the specified direction. Values got from those sensors are passed to the neural network, output of which determines the car's movement. There are two output neurons in the network, one for acceleration and the other for turning. In crossroads car uses the values got from GPS to choose the right direction. After development, cars (theirs neural networks) can be saved to files for future use.

## Screenshots
<p align="center">
  <img width="80%" alt="Car sensors" src="https://www.dropbox.com/s/hjhb5neb26cn52b/Screenshot_1.png?raw=1">
</p>
<p align="center">
  <img width="80%" alt="Training" src="https://www.dropbox.com/s/av2jcigrir8teef/Screenshot_2.png?raw=1">
</p>
<p align="center">
  <img width="80%" alt="Crossroad" src="https://www.dropbox.com/s/1snn5irxw5rwn2b/Screenshot_3.png?raw=1">
</p>

## Demo Videos

- [Training](https://drive.google.com/file/d/1IqalK3BaT8d9-iTwHDy4Pq9YJPdbj84-/view?usp=sharing)
- [Testing](https://drive.google.com/file/d/1Mj17uSxLAIi4BhNBJWJwut7RnTCBwfe5/view?usp=sharing)
- [Training [Crossroads]](https://drive.google.com/file/d/19tqK3NjgEiIhVRQi8QK9tpC1P7EU6yVd/view?usp=sharing)
- [Testing [Crossroads]](https://drive.google.com/file/d/1pUEzQAXMEcyUBbx1gY78JmiiMKJu9wMy/view?usp=sharing)
- [Autonomous Car vs Human](https://drive.google.com/file/d/1_4_KDoWBCe7Yt2xL0fCyIRppdpPwzyGf/view?usp=sharing)

## Credits
Thanks to [SebLague](https://github.com/SebLague) for ["[Unity] 2D Curve Editor"](https://www.youtube.com/playlist?list=PLFt_AvWsXl0d8aDaovNztYf6iTChHzrHP) tutorials.
   
