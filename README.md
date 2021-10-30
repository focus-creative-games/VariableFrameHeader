# VariableFrameHeader

以类似pb varint格式的length作帧头的无需提前知道body大小的O(1)额外消息体移动时间复杂度的帧格式方案。

## 介绍
一种最常见的帧格式为帧头部有一个length字段，指明帧消息体的大小，后面紧跟body。对于length字段，有固定长度和动态长度这两种常见方法：
- 固定长度。实现比较简单，但如果长度太小（比如2字节），很容易遇到大包体而不得不拆包，如果超出长度太大（比如4字节），则由于
大多数数据包其实只有几十字节，length字段浪费严重。
- 动态长度。一个经典算法如pb的varint算法，对于大多数较小长度的数据包不会浪费空间。但实践中，经常是边序列化边构造帧，**无法提前知道帧体的大小**，万一超出最小长度，则不得不移动整个包体，O(N)额外复杂度。

本算法解决对于无法提前确定帧体大小的动态长度在超出基本长度后的O(N)时间复杂度的帧消息体移动问题。

**本算法也适用于数据库不定长难以提前计算size的复合字段的序列化，[brightdb](https://github.com/focus-creative-games/BrightDB)序列化使用到了此算法**。

## 算法

以下面一种简单的帧格式为例介绍实现：

| length | body |

序列化：
- 先预留1字节的空间，接着序列化消息体内容
- 序列完消息体后，计算使用varint之类的算法序列化后消息体长度占用空间 sizeOfBodyLength
    - 如果 sizeOfBodyLength 长度为 1，则直接写入之前预留的空间，完成序列化
    - 如果 > 1，将body的前(sizeOfBodyLength-1)字节追加到消息尾部，同时BodyLength从预留空间写入。完成序列化

反序列化：
- 读取消息长度 bodyLength,同时获得bodyLength占用的空间sizeOfBodyLength
    - 如果 sizeOfBodyLength == 1，则后续长度为 bodyLength的内容既为消息体，完成反序列化。
    - 如果 sizeOfBodyLength > 1, 则将尾部 sizeOfBodyLength-1 的字节复制到本消息帧第1个字节（序号从0开始）开始的位置，帧 [1, sizeOfBody) 既为消息体，完成反序列化



## 实现
  见Frame类源码。
