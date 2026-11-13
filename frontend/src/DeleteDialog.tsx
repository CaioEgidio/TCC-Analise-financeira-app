import { useEffect, useRef, useState } from 'react'
import { deleteTransaction, SessionExpiredError } from './financial-api'
import type { Transaction } from './financial-api'
import './CreateDialog.css'

type Props = {
  token: string
  transaction: Transaction
  onClose: () => void
  onDeleted: () => void
  onSessionExpired: () => void
}

export default function DeleteDialog({ token, transaction, onClose, onDeleted, onSessionExpired }: Props) {
  const ref = useRef<HTMLDialogElement>(null)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  useEffect(() => {
    const dialog = ref.current
    dialog?.showModal()
    return () => dialog?.close()
  }, [])

  async function remove() {
    if (saving) return
    setSaving(true)
    setError('')
    try {
      await deleteTransaction(token, transaction.id)
      onDeleted()
    } catch (cause) {
      if (cause instanceof SessionExpiredError) { onSessionExpired(); return }
      setError(cause instanceof Error ? cause.message : 'Não foi possível excluir o lançamento.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <dialog ref={ref} className="create-dialog" aria-labelledby="delete-title"
      onCancel={(event) => { event.preventDefault(); if (!saving) onClose() }}>
      <div className="dialog-heading"><div>
        <h2 id="delete-title">Excluir lançamento?</h2>
        <p>Você vai excluir <strong>{transaction.description}</strong>, de {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(transaction.amount)}.</p>
      </div></div>
      <p>Esta ação é permanente. Os totais do dashboard serão recalculados.</p>
      {error && <p className="form-error" role="alert">{error}</p>}
      <div className="dialog-actions" aria-busy={saving}>
        <button type="button" className="secondary-button" disabled={saving} onClick={onClose} autoFocus>Cancelar</button>
        <button type="button" className="danger-button" disabled={saving} onClick={remove}>{saving ? 'Excluindo…' : 'Excluir lançamento'}</button>
      </div>
    </dialog>
  )
}
