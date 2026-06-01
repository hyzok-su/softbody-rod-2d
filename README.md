# Overview

This project is a Unity 2d softbody rod simulation program based on mass-spring system, additionally using joints, orthogonally decoupled damper, and topological tearing. By modifying the parameters, the framework can represent different material behaviors without changing topology, ranging from flexible rope-like motion to rigid beam-like structures and hinge-based pendulum motion.

<img src="./docs/wholeSystemDemo.gif" width="30%"/> 

<p>
<img src="./docs/nodampingRope.gif" height="250" width="20%"/>  <img src="./docs/damping&dragRope.gif" height="250" width="20%"/> <img src="./docs/dampingRope.gif" height="250" width="20%"/>
<p/>
  
<p>
<img src="./docs/nodamping.gif" height="250" width="20%"/>    <img src="./docs/damping&drag.gif" height="250" width="20%"/>   <img src="./docs/damping.gif" height="250" width="20%"/>
<p/>
  
<p>
<img src="./docs/nodampingPendulum.gif" height="200" width="30%"/>    <img src="./docs/conventionalDampingPendulum.gif" height="200" width="30%"/>   <img src="./docs/ourdampingPendulum.gif" height="200" width="30%"/>
<p/>

<p>
<img src="./docs/staticfriction.gif" height="235" width="20%"/>   <img src="./docs/nofriction.gif" height="235" width="20%"/>  <img src="./docs/fullrestitution.gif" height="235" width="20%"/>    <img src="./docs/norestitution.gif" height="235" width="20%"/>  
<p/>

<p>
<img src="./docs/hingeJoint.gif" height="235" width="30%"/>   <img src="./docs/partialJoint.gif" height="235" width="30%"/>  <img src="./docs/rigidJoint.gif" height="235" width="30%"/>   
<p/>

<p>
<img src="./docs/notearing.gif" height="300" width="30%"/>   <img src="./docs/tearing.gif" height="300" width="30%"/>  
<p/>
