import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { loadDashboard, SessionExpiredError } from './financial-api'
import type { DashboardData, Transaction } from './financial-api'
import CreateDialog from './CreateDialog'
import DeleteDialog from './DeleteDialog'
import './Dashboard.css'

type Props = {
  token: string
  email: string
  onLogout: () => void
  onSessionExpired: () => void
}

type Result = {
  requestKey: string
  data?: DashboardData
  error?: string
}

const currency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const monthNames = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro']

function initialPeriod() {
  const now = new Date()
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`
}

function formatDate(value: string) {
  const [year, month, day] = value.split('-')
  return `${day}/${month}/${year}`
}

export default function Dashboard({ token, email, onLogout, onSessionExpired }: Props) {
  const [period, setPeriod] = useState(initialPeriod)
  const [refresh, setRefresh] = useState(0)
  const [result, setResult] = useState<Result | null>(null)
  const [dialogMode, setDialogMode] = useState<'category' | 'transaction' | null>(null)
  const [notice, setNotice] = useState('')
  const [editing, setEditing] = useState<Transaction | undefined>()
  const [deleting, setDeleting] = useState<Transaction | null>(null)
  const requestKey = `${period}-${refresh}`
  const loading = result?.requestKey !== requestKey
  const data = loading ? undefined : result?.data
  const error = loading ? undefined : result?.error
  const [year, month] = period.split('-').map(Number)
  const periodLabel = `${monthNames[month - 1]} de ${year}`

  useEffect(() => {
    const controller = new AbortController()
    loadDashboard(token, period, controller.signal)
      .then((dashboard) => {
        if (!controller.signal.aborted) setResult({ requestKey, data: dashboard })
      })
      .catch((cause: unknown) => {
        if (controller.signal.aborted) return
        if (cause instanceof SessionExpiredError) {
          onSessionExpired()
          return
        }
        setResult({ requestKey, error: cause instanceof TypeError
          ? 'Não foi possível conectar ao FinData. Confira sua conexão e tente novamente.'
          : cause instanceof Error ? cause.message : 'Não foi possível carregar seus dados.' })
      })
    return () => controller.abort()
  }, [token, period, requestKey, onSessionExpired])

  function changePeriod(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const selected = String(new FormData(event.currentTarget).get('period'))
    if (!/^\d{4}-(0[1-9]|1[0-2])$/.test(selected) || selected.startsWith('0000')) return
    setPeriod(selected)
    setRefresh((value) => value + 1)
  }

  function handleSaved(date?: string) {
    setNotice(date ? 'Lançamento salvo. Os dados do período foram atualizados.' : 'Categoria salva. Ela já pode ser usada em novos lançamentos.')
    if (date) setPeriod(date.slice(0, 7))
    setRefresh((value) => value + 1)
    setDialogMode(null)
    setEditing(undefined)
  }

  return (
    <main className="dashboard" lang="pt-BR">
      <header className="dashboard-header">
        <div className="dashboard-header-inner">
          <a className="brand" href="/" aria-label="FinData, início">
            <span className="brand-mark" aria-hidden="true"><i /><i /><i /></span>FinData
          </a>
          <div className="dashboard-account">
            <span title={email}>{email}</span>
            <button className="logout-button" type="button" onClick={onLogout}>Sair da conta</button>
          </div>
        </div>
      </header>

      <div className="dashboard-content">
        <section className="dashboard-heading" aria-labelledby="dashboard-title">
          <div>
            <h1 id="dashboard-title">Seu mês, em perspectiva.</h1>
            <p>Acompanhe suas entradas, saídas e o saldo do período.</p>
          </div>
          <form key={period} className="period-form" onSubmit={changePeriod}>
            <label htmlFor="period">Período</label>
            <div className="period-controls">
              <input id="period" name="period" type="month" defaultValue={period}
                min="0001-01" max="9999-12" required />
              <button className="period-button" type="submit">Atualizar</button>
            </div>
          </form>
        </section>

        <div className="dashboard-actions" aria-label="Cadastrar dados financeiros">
          <button className="primary-button" type="button" onClick={() => { setNotice(''); setDialogMode('transaction') }}>Novo lançamento</button>
          <button className="secondary-button" type="button" onClick={() => { setNotice(''); setDialogMode('category') }}>Nova categoria</button>
        </div>
        {notice && <p className="save-notice" role="status">{notice}</p>}

        <p className="period-caption">{periodLabel}</p>
        {loading && <div className="dashboard-notice" role="status">Carregando os dados de {periodLabel.toLowerCase()}…</div>}
        {error && (
          <div className="dashboard-error" role="alert">
            <p>{error}</p>
            <button type="button" onClick={() => setRefresh((value) => value + 1)}>Tentar novamente</button>
          </div>
        )}

        {data && (
          <>
            <section className="summary-grid" aria-label="Resumo financeiro mensal">
              <article className="summary-item">
                <div className="summary-label"><span>Receitas</span><span className="income-symbol" aria-hidden="true">+</span></div>
                <strong className="summary-value income-value">{currency.format(data.summary.totalIncome)}</strong>
                <p>Total recebido no período</p>
              </article>
              <article className="summary-item">
                <div className="summary-label"><span>Despesas</span><span className="expense-symbol" aria-hidden="true">−</span></div>
                <strong className="summary-value expense-value">{currency.format(data.summary.totalExpense)}</strong>
                <p>Total gasto no período</p>
              </article>
              <article className="summary-item balance-item">
                <div className="summary-label"><span>Saldo do mês</span><span aria-hidden="true">=</span></div>
                <strong className="summary-value">{currency.format(data.summary.balance)}</strong>
                <p>Receitas menos despesas do período</p>
              </article>
            </section>

            <section className="dashboard-section" aria-labelledby="categories-title">
              <div className="section-heading">
                <div><h2 id="categories-title">Para onde foi seu dinheiro?</h2><p>Despesas agrupadas por categoria</p></div>
                <span className="section-total">{currency.format(data.summary.totalExpense)} em despesas</span>
              </div>
              {data.categories.length === 0 ? (
                <div className="dashboard-empty"><h3>Nenhuma despesa neste mês.</h3><p>As despesas do período aparecerão aqui, organizadas por categoria.</p></div>
              ) : (
                <ul className="category-breakdown">
                  {data.categories.map((category) => (
                    <li key={category.categoryId}>
                      <div className="category-caption"><span>{category.categoryName}</span><strong>{currency.format(category.total)}</strong></div>
                      <meter min={0} max={Math.max(data.summary.totalExpense, category.total, 1)} value={category.total}
                        aria-label={`Despesas em ${category.categoryName}`}>
                        {currency.format(category.total)}
                      </meter>
                    </li>
                  ))}
                </ul>
              )}
            </section>

            <section className="dashboard-section" aria-labelledby="transactions-title">
              <div className="section-heading">
                <div><h2 id="transactions-title">Movimentações do mês</h2><p>Receitas e despesas do período selecionado</p></div>
                <span className="section-total">{data.transactions.length} {data.transactions.length === 1 ? 'lançamento' : 'lançamentos'}</span>
              </div>
              {data.transactions.length === 0 ? (
                <div className="dashboard-empty"><h3>Seu mês começa com uma página em branco.</h3><p>Os lançamentos da sua conta aparecerão aqui quando forem cadastrados neste período.</p></div>
              ) : (
                <div className="transactions-table-wrapper" role="region" aria-label="Tabela de movimentações" tabIndex={0}>
                  <table className="transactions-table">
                    <caption className="visually-hidden">Movimentações de {periodLabel}</caption>
                    <thead><tr><th scope="col">Descrição</th><th scope="col">Categoria</th><th scope="col">Data</th><th scope="col">Tipo</th><th scope="col" className="amount-column">Valor</th><th scope="col">Ações</th></tr></thead>
                    <tbody>
                      {data.transactions.map((transaction) => (
                        <tr key={transaction.id}>
                          <th scope="row">{transaction.description}</th>
                          <td>{transaction.categoryName}</td>
                          <td><time dateTime={transaction.date}>{formatDate(transaction.date)}</time></td>
                          <td><span className={transaction.type === 1 ? 'type-income' : 'type-expense'}>{transaction.type === 1 ? 'Receita' : 'Despesa'}</span></td>
                          <td className="amount-column">{currency.format(transaction.amount)}</td>
                          <td><div className="row-actions">
                            <button type="button" className="secondary-button" aria-label={`Editar ${transaction.description}`} onClick={() => { setNotice(''); setEditing(transaction); setDialogMode('transaction') }}>Editar</button>
                            <button type="button" className="secondary-button delete-action" aria-label={`Excluir ${transaction.description}`} onClick={() => { setNotice(''); setDeleting(transaction) }}>Excluir</button>
                          </div></td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </section>
          </>
        )}
        <footer className="dashboard-footer">FinData · Controle financeiro pessoal</footer>
      </div>
      {dialogMode && <CreateDialog key={editing?.id ?? dialogMode} mode={dialogMode} token={token} period={period} transaction={editing}
        onClose={() => { setDialogMode(null); setEditing(undefined) }} onSaved={handleSaved} onSessionExpired={onSessionExpired}
        onCreateCategory={() => { setEditing(undefined); setDialogMode('category') }} />}
      {deleting && <DeleteDialog token={token} transaction={deleting} onClose={() => setDeleting(null)}
        onSessionExpired={onSessionExpired} onDeleted={() => {
          setDeleting(null)
          setNotice('Lançamento excluído. Os dados do período foram atualizados.')
          setRefresh((value) => value + 1)
        }} />}
    </main>
  )
}
