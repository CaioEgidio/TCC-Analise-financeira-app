import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { loadDashboard, SessionExpiredError } from './financial-api'
import type { DashboardData, Transaction } from './financial-api'
import CreateDialog from './CreateDialog'
import DeleteDialog from './DeleteDialog'
import UiIcon from './UiIcon'
import { categoryColors, expenseChartBackground } from './category-chart'
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
  const [activeSection, setActiveSection] = useState('overview')
  const requestKey = `${period}-${refresh}`
  const loading = result?.requestKey !== requestKey
  const data = loading ? undefined : result?.data
  const error = loading ? undefined : result?.error
  const [year, month] = period.split('-').map(Number)
  const periodLabel = `${monthNames[month - 1]} de ${year}`
  const totalExpenses = data?.summary.totalExpense ?? 0
  const chartBackground = expenseChartBackground(data?.categories ?? [], totalExpenses)

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
    <div className="dashboard" lang="pt-BR">
      <a className="skip-link" href="#dashboard-main">Ir para o conteúdo</a>
      <aside className="dashboard-sidebar" aria-label="Navegação FinData">
        <a className="brand" href="#dashboard-main" aria-label="FinData, início"><span className="brand-mark" aria-hidden="true"><i /><i /><i /></span>FinData<span className="brand-dot">.</span></a>
        <p className="sidebar-caption">Seu espaço financeiro</p>
        <nav className="dashboard-nav" aria-label="Seções do dashboard">
          <a href="#dashboard-main" aria-current={activeSection === 'overview' ? 'location' : undefined} onClick={() => setActiveSection('overview')}><UiIcon name="overview" />Visão geral</a>
          <a href="#transactions-title" aria-current={activeSection === 'transactions' ? 'location' : undefined} onClick={() => setActiveSection('transactions')}><UiIcon name="transactions" />Lançamentos</a>
          <a href="#categories-title" aria-current={activeSection === 'categories' ? 'location' : undefined} onClick={() => setActiveSection('categories')}><UiIcon name="category" />Por categoria</a>
        </nav>
        <div className="sidebar-bottom">
          <div className="sidebar-note"><UiIcon name="wallet" size={24} /><strong>Organização que faz diferença.</strong><p>Um lançamento de cada vez, mais clareza para decidir.</p></div>
          <button type="button" className="sidebar-category" onClick={() => { setNotice(''); setDialogMode('category') }}><UiIcon name="plus" size={18} />Criar categoria</button>
        </div>
      </aside>
      <header className="dashboard-header">
        <div className="dashboard-header-inner">
          <span className="header-location">Meu financeiro <span aria-hidden="true">/</span> <strong>Visão geral</strong></span>
          <div className="dashboard-account">
            <span className="account-avatar" aria-hidden="true">{email.slice(0, 1).toUpperCase()}</span>
            <span title={email}>{email}</span>
            <button className="logout-button" type="button" onClick={onLogout}><UiIcon name="logout" size={17} /><span>Sair da conta</span></button>
          </div>
        </div>
      </header>

      <main id="dashboard-main" className="dashboard-content" tabIndex={-1}>
        <section className="dashboard-heading" aria-labelledby="dashboard-title">
          <div>
            <h1 id="dashboard-title">Visão geral</h1>
            <p>Seu dinheiro, com tudo no lugar.</p>
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
          <button className="primary-button" type="button" onClick={() => { setNotice(''); setDialogMode('transaction') }}><UiIcon name="plus" size={18} />Novo lançamento</button>
          <button className="secondary-button" type="button" onClick={() => { setNotice(''); setDialogMode('category') }}><UiIcon name="category" size={17} />Nova categoria</button>
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
            <section key={requestKey} className="summary-grid" aria-label="Resumo financeiro mensal">
              <article className="summary-item">
                <div className="summary-label"><span>Receitas</span><span className="income-symbol"><UiIcon name="up" /></span></div>
                <strong className="summary-value income-value">{currency.format(data.summary.totalIncome)}</strong>
                <p>Total recebido no período</p>
              </article>
              <article className="summary-item">
                <div className="summary-label"><span>Despesas</span><span className="expense-symbol"><UiIcon name="down" /></span></div>
                <strong className="summary-value expense-value">{currency.format(data.summary.totalExpense)}</strong>
                <p>Total gasto no período</p>
              </article>
              <article className="summary-item balance-item">
                <div className="summary-label"><span>Saldo do mês</span><span className="balance-symbol"><UiIcon name="wallet" /></span></div>
                <strong className="summary-value">{currency.format(data.summary.balance)}</strong>
                <p>Receitas menos despesas do período</p>
              </article>
            </section>

            <section className="dashboard-section" aria-labelledby="categories-title">
              <div className="section-heading">
                <div><h2 id="categories-title" tabIndex={-1}>Despesas por categoria</h2><p>Entenda o que compõe seus gastos</p></div>
                <span className="section-total">{currency.format(data.summary.totalExpense)} em despesas</span>
              </div>
              {data.categories.length === 0 ? (
                <div className="dashboard-empty"><h3>Nenhuma despesa neste mês.</h3><p>As despesas do período aparecerão aqui, organizadas por categoria.</p></div>
              ) : (
                <div className="category-layout">
                  <div className="expense-chart" aria-hidden="true"><div className="expense-ring" style={{ background: chartBackground }}><div><span>Total de despesas</span><strong>{currency.format(totalExpenses)}</strong></div></div></div>
                <ul className="category-breakdown">
                  {data.categories.map((category, index) => (
                    <li key={category.categoryId}>
                      <div className="category-caption"><span><i className="category-dot" style={{ background: categoryColors[index % categoryColors.length] }} />{category.categoryName}</span><strong>{currency.format(category.total)}</strong></div>
                      <meter min={0} max={Math.max(data.summary.totalExpense, category.total, 1)} value={category.total}
                        aria-label={`Despesas em ${category.categoryName}`}>
                        {currency.format(category.total)}
                      </meter>
                    </li>
                  ))}
                </ul>
                </div>
              )}
            </section>

            <section className="dashboard-section" aria-labelledby="transactions-title">
              <div className="section-heading">
                <div><h2 id="transactions-title" tabIndex={-1}>Lançamentos</h2><p>Todos os movimentos do período</p></div>
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
                            <button type="button" className="secondary-button" aria-label={`Editar ${transaction.description}`} onClick={() => { setNotice(''); setEditing(transaction); setDialogMode('transaction') }}><UiIcon name="edit" size={16} /><span>Editar</span></button>
                            <button type="button" className="secondary-button delete-action" aria-label={`Excluir ${transaction.description}`} onClick={() => { setNotice(''); setDeleting(transaction) }}><UiIcon name="trash" size={16} /><span>Excluir</span></button>
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
      </main>
      {dialogMode && <CreateDialog key={editing?.id ?? dialogMode} mode={dialogMode} token={token} period={period} transaction={editing}
        onClose={() => { setDialogMode(null); setEditing(undefined) }} onSaved={handleSaved} onSessionExpired={onSessionExpired}
        onCreateCategory={() => { setEditing(undefined); setDialogMode('category') }} />}
      {deleting && <DeleteDialog token={token} transaction={deleting} onClose={() => setDeleting(null)}
        onSessionExpired={onSessionExpired} onDeleted={() => {
          setDeleting(null)
          setNotice('Lançamento excluído. Os dados do período foram atualizados.')
          setRefresh((value) => value + 1)
        }} />}
    </div>
  )
}
