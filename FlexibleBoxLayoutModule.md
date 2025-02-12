

### Flexbox Container

| Flexbox Container 属性 | 作用               |
| ---------------------- | ------------------ |
| FlexDirection          | 主轴方向           |
| FlexWrap               | 换行               |
| MainAlignment          | 主轴对齐方式       |
| CrossAlignment         | 交叉轴单行对齐方式 |
| CrossContentAlignment  | 交叉轴多行对其方式 |

### Flexbox Item

| Flexbox Item 属性 | 作用                                              |
| ----------------- | ------------------------------------------------- |
| FlexGrow          | 子元素总尺寸小于父元素 (max-content)/sum * grow   |
| FlexShrink        | 子元素总尺寸大于父元素 (content-max)/sum * shrink |
| FlexBasis         | 定义项目初始大小 `don't`                          |
| FlexAlignSelf     | 定义项目单独对齐 `don't`                          |


### `MainAlignment` 主轴对其方式

| 名称          | 效果                              |
| ------------- | --------------------------------- |
| start         | row:靠左对齐 col:靠上对齐         |
| end           | row:靠右对齐 col:靠下对齐         |
| center        | 居中对其                          |
| space-between | 两端对其                          |
| space-evenly  | 效果: [1]-[1]-[1]-[1]-[1]         |
| space-around  | 效果: [1]-[2]-[2]-[2]-[1] `don't` |

### `CrossAlignment` 交叉轴对其方式

| 名称    | 效果     |
| ------- | -------- |
| start   | 靠近头部 |
| center  | 居中     |
| end     | 靠近尾部 |
| stretch | 拉伸填满 |

