export type FinancialSummary = {
  year: number
  month: number
  totalIncome: number
  totalExpense: number
  balance: number
}

export type CategoryExpense = {
  categoryId: string
  categoryName: string
  total: number
}

export type Transaction = {
  id: string
  description: string
  amount: number
  date: string
  type: 1 | 2
  categoryId: string
  categoryName: string
}

export type DashboardData = {
  summary: FinancialSummary
  categories: CategoryExpense[]
  transactions: Transaction[]
}

export type Category = { id: string; name: string; description: string | null }
export type NewTransaction = {
  description: string
  amount: number
  date: string
  type: 1 | 2
  categoryId: string
}

export class SessionExpiredError extends Error {}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function isAmount(value: unknown): value is number {
  return typeof value === 'number' && Number.isFinite(value)
}

function isSummary(value: unknown): value is FinancialSummary {
  return isRecord(value) && typeof value.year === 'number' &&
    typeof value.month === 'number' && isAmount(value.totalIncome) &&
    isAmount(value.totalExpense) && isAmount(value.balance)
}

function isCategoryExpense(value: unknown): value is CategoryExpense {
  return isRecord(value) && typeof value.categoryId === 'string' &&
    typeof value.categoryName === 'string' && isAmount(value.total) && value.total >= 0
}

function isTransaction(value: unknown): value is Transaction {
  return isRecord(value) && typeof value.id === 'string' &&
    typeof value.description === 'string' && isAmount(value.amount) &&
    typeof value.date === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value.date) &&
    (value.type === 1 || value.type === 2) &&
    typeof value.categoryId === 'string' && typeof value.categoryName === 'string'
}

async function readJson(path: string, token: string, signal: AbortSignal): Promise<unknown> {
  const response = await fetch(path, {
    headers: { Authorization: `Bearer ${token}` },
    signal,
  })
  if (response.status === 401) throw new SessionExpiredError('Sua sessão expirou. Entre novamente.')
  if (!response.ok) throw new Error('Não foi possível carregar os dados. Tente novamente.')
  return response.json()
}

export async function loadDashboard(
  token: string,
  period: string,
  signal: AbortSignal,
): Promise<DashboardData> {
  const [year, month] = period.split('-').map(Number)
  const query = new URLSearchParams({ year: String(year), month: String(month) })
  const [summary, categories, transactions] = await Promise.all([
    readJson(`/api/dashboard/summary?${query}`, token, signal),
    readJson(`/api/dashboard/expenses-by-category?${query}`, token, signal),
    readJson(`/api/transactions?${query}`, token, signal),
  ])

  if (!isSummary(summary) || !Array.isArray(categories) ||
      !categories.every(isCategoryExpense) || !Array.isArray(transactions) ||
      !transactions.every(isTransaction) || summary.year !== year || summary.month !== month) {
    throw new Error('Os dados recebidos estão incompletos. Tente atualizar o período.')
  }
  return { summary, categories, transactions }
}

export async function getCategories(token: string, signal: AbortSignal): Promise<Category[]> {
  const data = await readJson('/api/categories', token, signal)
  if (!Array.isArray(data) || !data.every((item: unknown) => isRecord(item) &&
      typeof item.id === 'string' && typeof item.name === 'string' &&
      (item.description === null || typeof item.description === 'string'))) {
    throw new Error('Não foi possível ler suas categorias. Tente novamente.')
  }
  return data
}

async function writeRecord(path: string, token: string, method: 'POST' | 'PUT' | 'DELETE', body?: object): Promise<Response> {
  let response: Response
  try {
    response = await fetch(path, {
      method,
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new Error('A conexão foi interrompida. Confira se o registro foi salvo antes de tentar novamente.')
  }
  if (response.status === 401) throw new SessionExpiredError('Sua sessão expirou. Entre novamente.')
  if (!response.ok) {
    const problem: unknown = await response.json().catch(() => null)
    if (isRecord(problem) && typeof problem.detail === 'string') throw new Error(problem.detail)
    if (isRecord(problem) && isRecord(problem.errors)) {
      const messages = Object.values(problem.errors).flat().filter((value): value is string => typeof value === 'string')
      if (messages.length) throw new Error(messages.join(' '))
    }
    throw new Error('Não foi possível salvar. Confira os dados e tente novamente.')
  }
  return response
}

async function createRecord(path: string, token: string, body: object): Promise<string> {
  const response = await writeRecord(path, token, 'POST', body)
  const created: unknown = await response.json().catch(() => null)
  if (!isRecord(created) || typeof created.id !== 'string' || !created.id) {
    throw new Error('Não foi possível confirmar o cadastro. Confira seus dados antes de tentar novamente.')
  }
  return created.id
}

export function createCategory(token: string, name: string, description: string) {
  return createRecord('/api/categories', token, { name: name.trim(), description: description.trim() || null })
}

export function createTransaction(token: string, transaction: NewTransaction) {
  return createRecord('/api/transactions', token, transaction)
}

export async function updateTransaction(token: string, id: string, transaction: NewTransaction) {
  await writeRecord(`/api/transactions/${encodeURIComponent(id)}`, token, 'PUT', transaction)
}

export async function deleteTransaction(token: string, id: string) {
  await writeRecord(`/api/transactions/${encodeURIComponent(id)}`, token, 'DELETE')
}
