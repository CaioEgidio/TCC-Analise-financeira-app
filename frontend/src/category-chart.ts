import type { CategoryExpense } from './financial-api'

export const categoryColors = ['#82a7ff', '#5fe0ba', '#b49aff', '#ffc27a', '#65cde4', '#f394b7']

export function expenseChartBackground(categories: CategoryExpense[], total: number) {
  if (total <= 0 || categories.length === 0) return '#26354d'
  let position = 0
  const segments = categories.map((category, index) => {
    const start = position
    position += category.total / total * 100
    return `${categoryColors[index % categoryColors.length]} ${start}% ${position}%`
  })
  return `conic-gradient(${segments.join(', ')})`
}
