# TinyLands — Forest Design

## 1. Общая концепция

Forest — визуальный биом из 5 постоянных схем размещения.

Каждая схема последовательно развивается от Level 1 до Level 3. При повышении уровня номер схемы сохраняется, меняется только её композиция.

Схема выбирается случайно, без учёта соседних сегментов.

## 2. Ассеты

Источник: https://obviouslydracula.itch.io/low-poly-pixel-art-spruce-trees

Папка: `Assets/Models/LowPolySpruce`

Исходные файлы:
- `SpruceTrees.blend`
- `SpruceTrees.glb`
- `textures/SpruceAlphas256.png`
- `SpruceBase256.png`

Созданы все 13 отдельных Prefab:

- `Spruce1–Spruce6`
- `DeadSpruce`
- `SpruceSapling1`
- `SpruceSapling2`
- `SpruceBush`
- `Flowers1`
- `Grass1`
- `Grass2`

Все Prefab доступны для визуальной системы.

## 3. Характеристики моделей

Ориентировочные размеры, полученные через временные Box Collider:

| Модель | Size X | Size Y | Size Z |
|---|---:|---:|---:|
| Spruce1 | 5.00 | 5.28 | 10.98 |
| Spruce2 | 4.06 | 5.48 | 11.04 |
| Spruce3 | 5.08 | 5.04 | 11.83 |
| Spruce4 | 5.09 | 5.02 | 16.27 |
| Spruce5 | 5.04 | 4.75 | 15.07 |
| Spruce6 | 4.96 | 4.72 | 16.13 |
| Sapling1 | 2.92 | 3.11 | 4.41 |
| Sapling2 | 2.87 | 3.05 | 6.06 |
| SpruceBush | 3.25 | 3.09 | 1.96 |
| DeadSpruce | 4.06 | 5.48 | 11.04 |
| Flowers1 | 0.48 | 0.40 | 0.59 |
| Grass1 | 0.48 | 0.59 | 0.59 |
| Grass2 | 0.48 | 0.40 | 0.59 |

Размеры ориентировочные, не являются точными границами видимой геометрии.

### Оси и ориентация

- В исходных моделях локальная Z направлена вдоль ствола.
- Исходный Quaternion: `(-0.707107, 0, 0, 0.707107)`.
- Соответствует Rotation X = -90°.
- После поворота локальная Size Z приблизительно соответствует мировой высоте дерева.

## 4. Проверка Prefab

Подтверждено:

**Spruce1**
- Pivot у основания.
- Вертикальное положение.
- Scale `(1,1,1)`.
- Локальная Z направлена вдоль ствола.
- Временный Box Collider удалён.

**SpruceSapling1**
- Pivot у основания.
- Вертикальное положение.
- Scale `(1,1,1)`.

**SpruceSapling2**
- Pivot у основания.
- Вертикальное положение.
- Scale `(1,1,1)`.

Остальные модели отдельно по Pivot и ориентации пока не проверены.

## 5. Постоянные схемы развития Forest

### Схема 1 — Полянка

- L1: SpruceSapling1, Grass1, Flowers1.
- L2: Spruce1, Grass2, Flowers1.
- L3: Spruce4, Grass1, Flowers1.

### Схема 2 — Подлесок

- L1: SpruceSapling1, SpruceBush.
- L2: Spruce2, Grass1, Flowers1.
- L3: Spruce5, SpruceBush, Grass2.

### Схема 3 — Цветущая поляна

- L1: SpruceSapling1, SpruceSapling2, Grass2, Flowers1.
- L2: Spruce1, Spruce3, Grass1, Flowers1.
- L3: Spruce4, Spruce6, Grass2, Flowers1.

### Схема 4 — Молодая роща

- L1: SpruceSapling1, SpruceSapling2, SpruceBush.
- L2: Spruce1, Spruce2, SpruceBush.
- L3: Spruce4, Spruce5, DeadSpruce, SpruceBush.

### Схема 5 — Зелёный уголок

- L1: SpruceSapling1, SpruceBush, Grass1, Flowers1.
- L2: Spruce3, SpruceBush, Grass2, Flowers1.
- L3: Spruce6, DeadSpruce, SpruceBush, Grass1, Grass2, Flowers1.

## 6. Согласованные правила

1. Используются 5 постоянных схем.
2. Номер схемы сохраняется при переходе между уровнями.
3. Схема выбирается случайно.
4. Соседние сегменты при выборе схемы не учитываются.
5. Grass1 и Grass2 чередуются в композициях.
6. Деревья могут выступать за границы сегмента, формируя общую композицию леса.
7. Все 13 Prefab доступны для визуальной системы.

## 7. Пока не согласовано

Не фиксированы:
- количество экземпляров каждой модели;
- масштабы объектов;
- плотность растительности;
- точные позиции;
- допустимые зоны размещения;
- правила случайного смещения объектов.

Эти параметры будут настраиваться после визуального осмотра композиций непосредственно в Unity.

Не требуется обязательное увеличение плотности при каждом переходе уровня.
