## 属性定义

### Flexbox Container

| 属性                    | 类型           | 作用               |
| ----------------------- | -------------- | ------------------ |
| `FlexDirection`         | `row` `column` | 主轴方向           |
| `FlexWrap`              | `boolean`      | 换行               |
| `MainAlignment`         | `enum`         | 主轴对齐方式       |
| `CrossAlignment`        | `enum`         | 交叉轴单行对齐方式 |
| `CrossContentAlignment` | `enum`         | 交叉轴多行对其方式 |

### Flexbox Item

1. `FlexGrow` 压缩子元素: (child - parent) / sumGrow * grow
2. `FlexShrink` 拉伸子元素: (parent - child) / sumShrink * shrink

| 属性            | 作用                       |
| --------------- | -------------------------- |
| `FlexGrow`      | 子元素总尺寸大于父元素生效 |
| `FlexShrink`    | 子元素总尺寸小于父元素生效 |
| `FlexBasis`     | 定义项目初始大小 `don't`   |
| `FlexAlignSelf` | 定义项目单独对齐 `don't`   |

## 属性效果

### `MainAlignment` 主轴对其方式

| 名称            | 效果                      |
| --------------- | ------------------------- |
| `start`         | 头部对齐                  |
| `end`           | 尾部对齐                  |
| `center`        | 居中对其                  |
| `space-between` | 两端对其                  |
| `space-evenly`  | 效果: [1]-[1]-[1]         |
| `space-around`  | 效果: [1]-[2]-[1] `don't` |

### `CrossAlignment` 交叉轴对其方式

| 名称      | 效果 |
| --------- | ---- |
| `start`   | 开始 |
| `center`  | 居中 |
| `end`     | 结尾 |
| `stretch` | 拉伸 |

