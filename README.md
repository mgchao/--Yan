## 衍 
#### 旨在逐步提高生命游戏模拟程序的运行效率。
***
#### 如何开始？

选择 [GameOfLife](https://github.com/mgchao/Yan/tree/GameOfLife) 分支获取采用 C# 语言编写的基于 [HashLife](https://www.conwaylife.com/wiki/HashLife) 算法的生命游戏的初步实现。

#### 为什么要有这个项目？

生命游戏的魅力令我着迷。起初我试图通过自己编写代码来体验生命游戏给我带来的种种震撼，但我拙劣的编程技术无法让我实现诸如 Golly 一般的高效模拟。
所以我开始研究 Golly 的源代码，然而可惜的是 Golly 的源代码已经高度优化，限于我的能力我很难理解其内部的许多实现。
在我查找其它的生命游戏算法资料时，我有幸读到了 Tomas G. Rokicki 的 《An Algorithm for Compressing Space and Time》，这篇文章让我初步理解了 HashLife 算法。

当我敲完 HashLife 的初步代码后，我无疑是倍感高兴的，也得益于我代码中的的诸多 Bug ，我更为深刻地理解了 HashLife 算法。同时，我也知道，如果我要更为高效地模拟生命游戏的话，在此基础上我还有很多的事要做。而这个项目便是**旨在逐步提高生命游戏模拟程序的运行效率**。

提高的每一步，都将在此记录……

#### 计划

虽然我非常喜爱 C# ，但它无疑不是高效实现生命游戏的最佳选择。我计划利用 Rust 来进一步提高程序的运行效率。