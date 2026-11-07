# Kenney Retro Medieval Kit — реестр моделей

## Общая информация

- Всего моделей: 105
- Анимаций: 0
- Лицензия: CC0 1.0 Universal
- Текстуры: отдельная папка `Textures`
- Форматы исходных моделей: OBJ, GLB, FBX

## 1. Стены и фасады

- `wall`
- `wall-detail`
- `wall-door`
- `wall-flat-gate`
- `wall-fortified`
- `wall-fortified-door`
- `wall-fortified-gate`
- `wall-fortified-gate-half`
- `wall-fortified-half`
- `wall-fortified-paint`
- `wall-fortified-paint-door`
- `wall-fortified-paint-gate`
- `wall-fortified-paint-gate-half`
- `wall-fortified-paint-half`
- `wall-fortified-paint-window`
- `wall-fortified-window`
- `wall-gate`
- `wall-gate-half`
- `wall-half`
- `wall-low`
- `wall-paint`
- `wall-paint-detail`
- `wall-paint-door`
- `wall-paint-flat`
- `wall-paint-gate`
- `wall-paint-half`
- `wall-paint-window`
- `wall-pane`
- `wall-pane-door`
- `wall-pane-paint`
- `wall-pane-paint-door`
- `wall-pane-paint-window`
- `wall-pane-painted-wood`
- `wall-pane-painted-wood-door`
- `wall-pane-painted-wood-window`
- `wall-pane-window`
- `wall-pane-wood`
- `wall-pane-wood-door`
- `wall-pane-wood-window`
- `wall-window`

## 2. Крыши

- `roof`
- `roof-corner`
- `roof-edge`
- `roof-high-side`
- `roof-high-side-corner`
- `roof-high-side-corner-inner`
- `roof-side`
- `roof-side-corner`
- `roof-side-corner-inner`

## 3. Каркас и опоры

- `structure`
- `structure-cross`
- `structure-pole`
- `structure-poles`
- `structure-wall`
- `structure-wall-cross`

## 4. Башни и укрепления

- `tower`
- `tower-base`
- `tower-edge`
- `tower-paint`
- `tower-paint-base`
- `tower-top`
- `battlement`
- `battlement-corner-inner`
- `battlement-corner-outer`
- `battlement-half`

## 5. Полы, лестницы и настилы

- `floor`
- `floor-flat`
- `floor-stairs`
- `floor-stairs-corner-inner`
- `floor-stairs-corner-outer`
- `floor-steps`
- `floor-steps-corner-inner`
- `floor-steps-corner-outer`
- `stairs-corner`
- `stairs-stone`
- `stairs-wood`
- `wood-floor`
- `wood-floor-half`
- `wood-floor-quarter`
- `wood-floor-railing`

## 6. Колонны и архитектурные детали

- `column`
- `column-damaged`
- `column-paint`
- `column-paint-damaged`
- `column-wood`
- `overhang`
- `overhang-fence`
- `overhang-round`
- `overhang-round-railing`

## 7. Заборы и ограждения

- `fence`
- `fence-top`
- `fence-wood`

## 8. Декор и предметы

- `barrels`
- `bricks`
- `detail-barrel`
- `detail-crate`
- `detail-crate-ropes`
- `detail-crate-small`
- `ladder`
- `pulley`
- `pulley-crate`

## 9. Причалы и вода

- `dock-corner`
- `dock-side`
- `water`

## 10. Природа

- `tree-large`
- `tree-shrub`

---

## Проверенные параметры моделей

| Модель | Размер | Примечание |
|---|---|---|
| `wall` | 1×1×1 | Объёмная стена |
| `wall-pane` | 1×1×0 | Плоская стена, Pivot в центре основания |
| `wall-window` | 1×1×1 | Box Collider охватывает модель |
| `roof` | 1×1×1 | Pivot в центре основания |
| `roof-side` | 1×0.5×1 | Один скат, модель усечена с трёх сторон |
| `roof-side-corner` | 1×0.5×1 | Угловая модель, два ската вниз в стороны, две усечённые стороны |
| `roof-side-corner-inner` | 1×0.5×1 | Внутренний угловой элемент, скаты направлены внутрь и вниз |
| `roof-high-side` | 1×1×1 | Высокий кровельный элемент с одним скатом |
| `roof-high-side-corner` | 1×1×1 | Высокий угловой кровельный элемент |
| `roof-high-side-corner-inner` | 1×1×1 | Высокий внутренний угловой элемент |

### Проверка соединений низкой кровли

Проверены следующие сочетания:

| Соединение | Результат |
|---|---|
| `roof-side` + `roof-side-corner` | Успешно |
| Два соседних `roof-side-corner` | Успешно |
| Четыре `roof-side-corner` | Образуют замкнутую конструкцию |
| `roof-side-corner-inner` + `roof-side` | Успешно |
| `roof-side-corner-inner` + `roof-side-corner` | Успешно |

Дополнительные наблюдения:

- `roof-side-corner-inner` имеет те же габариты, что и `roof-side-corner`.
- Основное отличие — направление скатов внутрь и вниз.
- Внутренний угол соединяется по той же логике, что и обычный угловой элемент.

### Проверка соединений высокой кровли

Все три модели высокой кровельной группы имеют размер 1×1×1. Габариты идеально подогнаны, элементы хорошо совмещаются.

Проверены следующие сочетания:

| Соединение | Результат |
|---|---|
| `roof-high-side` + `roof-high-side-corner` | Успешно |
| Два соседних `roof-high-side-corner` | Успешно |
| Четыре `roof-high-side-corner` | Образуют замкнутую конструкцию |
| `roof-high-side-corner-inner` + `roof-high-side` | Успешно |
| `roof-high-side-corner-inner` + `roof-high-side-corner` | Успешно, сочетание выглядит эстетично |

Дополнительные наблюдения:

- Высокая кровельная группа не содержит модели `roof-high`.
- Все модели группы имеют габариты 1×1×1.
- Соединения прямых и угловых элементов проверены.
- Внутренний угол совместим с прямым и внешним угловым элементами.
- Замкнутая конструкция из четырёх внешних угловых элементов собирается успешно.

### Общие правила набора

- Стандартные модели имеют размер `1×1×1`.
- Модели с суффиксом `-pane` имеют размер `1×1×0`.
- Модели совмещаются между собой.
- Стандартный Scale: `(1, 1, 1)`.
- Повторная проверка каждой стандартной модели не требуется.
- Модели с необычной геометрией проверяются отдельно.

Примечание: общие правила размеров основаны на проведённых проверках и наблюдениях при работе с набором. Не все модели проверялись индивидуально.
