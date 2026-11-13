import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { createCategory, createTransaction, updateTransaction, getCategories, SessionExpiredError } from './financial-api'
import type { Category, Transaction, NewTransaction } from './financial-api'
import './CreateDialog.css'

type Props = {
  mode: 'category' | 'transaction'
  token: string
  period: string
  transaction?: Transaction
  onClose: () => void
  onSaved: (date?: string) => void
  onSessionExpired: () => void
  onCreateCategory: () => void
}

function defaultDate(period: string) {
  const today = new Date()
  const localDate = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`
  return localDate.startsWith(period) ? localDate : `${period}-01`
}

export default function CreateDialog({ mode, token, period, transaction, onClose, onSaved, onSessionExpired, onCreateCategory }: Props) {
  const dialogRef = useRef<HTMLDialogElement>(null)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [categoryState, setCategoryState] = useState<{ data?: Category[]; error?: string } | null>(null)
  const [retry, setRetry] = useState(0)
  const isCategory = mode === 'category'
  const categories = categoryState?.data

  useEffect(() => {
    const dialog = dialogRef.current
    dialog?.showModal()
    return () => dialog?.close()
  }, [])

  useEffect(() => {
    if (isCategory) return
    const controller = new AbortController()
    getCategories(token, controller.signal)
      .then((data) => { if (!controller.signal.aborted) setCategoryState({ data }) })
      .catch((cause: unknown) => {
        if (controller.signal.aborted) return
        if (cause instanceof SessionExpiredError) { onSessionExpired(); return }
        setCategoryState({ error: 'Não foi possível carregar suas categorias.' })
      })
    return () => controller.abort()
  }, [isCategory, token, retry, onSessionExpired])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (saving) return
    const fields = new FormData(event.currentTarget)
    setError('')
    setSaving(true)
    try {
      if (isCategory) {
        await createCategory(token, String(fields.get('name')), String(fields.get('description') ?? ''))
        onSaved()
      } else {
        const amount = Number(fields.get('amount'))
        if (!Number.isFinite(amount) || amount <= 0) throw new Error('Informe um valor maior que zero.')
        // Evita arredondar valores que o JavaScript não representa com precisão de centavos.
        if (!Number.isSafeInteger(Math.round(amount * 100))) {
          throw new Error('O valor é muito alto para ser representado com precisão neste formulário.')
        }
        const date = String(fields.get('date'))
        const values: NewTransaction = {
          description: String(fields.get('description')).trim(),
          amount,
          date,
          type: fields.get('type') === '1' ? 1 : 2,
          categoryId: String(fields.get('categoryId')),
        }
        if (transaction) await updateTransaction(token, transaction.id, values)
        else await createTransaction(token, values)
        onSaved(date)
      }
    } catch (cause) {
      if (cause instanceof SessionExpiredError) { onSessionExpired(); return }
      setError(cause instanceof Error ? cause.message : 'Não foi possível salvar. Tente novamente.')
    } finally {
      setSaving(false)
    }
  }

  function close() {
    if (!saving) onClose()
  }

  return (
    <dialog className="create-dialog" ref={dialogRef} aria-labelledby="create-title"
      onCancel={(event) => { event.preventDefault(); close() }}>
      <div className="dialog-heading">
        <div>
          <h2 id="create-title">{isCategory ? 'Nova categoria' : transaction ? 'Editar lançamento' : 'Novo lançamento'}</h2>
          <p>{isCategory ? 'Organize seus lançamentos com um nome fácil de reconhecer.' : 'Registre uma receita ou despesa na sua conta.'}</p>
        </div>
        <button className="dialog-close" type="button" onClick={close} disabled={saving} aria-label="Fechar formulário">×</button>
      </div>

      {!isCategory && !categoryState && <p className="dialog-message" role="status">Carregando suas categorias…</p>}
      {!isCategory && categoryState?.error && (
        <div className="dialog-message" role="alert"><p>{categoryState.error}</p>
          <button className="secondary-button" type="button" onClick={() => setRetry((value) => value + 1)}>Tentar novamente</button>
        </div>
      )}
      {!isCategory && categories?.length === 0 && (
        <div className="dialog-message"><h3>Primeiro, crie uma categoria.</h3>
          <p>Você precisa de uma categoria da sua conta para organizar este lançamento.</p>
          <button className="primary-button" type="button" onClick={onCreateCategory}>Criar categoria</button>
        </div>
      )}

      {(isCategory || (categories && categories.length > 0)) && (
        <form onSubmit={submit} aria-busy={saving}>
          <fieldset disabled={saving} className="dialog-fields">
            {isCategory ? (
              <>
                <div className="form-field"><label htmlFor="category-name">Nome da categoria</label>
                  <input id="category-name" name="name" maxLength={100} required placeholder="Ex.: Alimentação" />
                </div>
                <div className="form-field"><label htmlFor="category-description">Descrição <span className="optional-label">(opcional)</span></label>
                  <textarea id="category-description" name="description" maxLength={500} rows={3} placeholder="Ex.: Mercado e restaurantes" />
                </div>
              </>
            ) : (
              <>
                <div className="form-field"><label htmlFor="transaction-description">Descrição</label>
                  <input id="transaction-description" name="description" maxLength={250} required defaultValue={transaction?.description} placeholder="Ex.: Compra no mercado" />
                </div>
                <div className="dialog-columns">
                  <div className="form-field"><label htmlFor="transaction-type">Tipo</label>
                    <select id="transaction-type" name="type" defaultValue={transaction?.type ?? 2}><option value="2">Despesa</option><option value="1">Receita</option></select>
                  </div>
                  <div className="form-field"><label htmlFor="transaction-amount">Valor (R$)</label>
                    <input id="transaction-amount" name="amount" type="number" inputMode="decimal" required min="0.01" step="0.01" defaultValue={transaction?.amount} placeholder="0,00" />
                  </div>
                </div>
                <div className="form-field"><label htmlFor="transaction-date">Data</label>
                  <input id="transaction-date" name="date" type="date" required min="0001-01-01" max="9999-12-31" defaultValue={transaction?.date ?? defaultDate(period)} />
                </div>
                <div className="form-field"><label htmlFor="transaction-category">Categoria</label>
                  <select id="transaction-category" name="categoryId" defaultValue={transaction?.categoryId ?? ''} required>
                    <option value="" disabled>Selecione uma categoria</option>
                    {categories?.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
                  </select>
                </div>
              </>
            )}
          </fieldset>
          {error && <p className="form-error" role="alert">{error}</p>}
          <div className="dialog-actions">
            <button className="secondary-button" type="button" onClick={close} disabled={saving}>Cancelar</button>
            <button className="primary-button" type="submit" disabled={saving}>
              {saving ? 'Salvando…' : isCategory ? 'Salvar categoria' : 'Salvar lançamento'}
            </button>
          </div>
        </form>
      )}
    </dialog>
  )
}
