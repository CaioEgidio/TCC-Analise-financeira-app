import { useState } from 'react'
import type { FormEvent } from 'react'
import { registerUser } from './registration-api'

type Props = { initialEmail: string; onBack: () => void; onRegistered: (email: string) => void }

export default function RegisterForm({ initialEmail, onBack, onRegistered }: Props) {
  const [showPassword, setShowPassword] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (submitting) return
    const fields = new FormData(event.currentTarget)
    const email = String(fields.get('email')).trim()
    setError('')
    setSubmitting(true)
    try {
      await registerUser(email, String(fields.get('password')), String(fields.get('confirmation')))
      onRegistered(email)
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Não foi possível criar a conta.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <>
      <h2 id="access-title">Crie sua conta.</h2>
      <p className="form-intro">Comece a organizar suas receitas e despesas no FinData.</p>
      <form onSubmit={submit} aria-busy={submitting}>
        <fieldset className="registration-fields" disabled={submitting}>
          <div className="form-field">
            <label htmlFor="register-email">E-mail</label>
            <input id="register-email" name="email" type="email" autoComplete="username" required maxLength={256}
              defaultValue={initialEmail} placeholder="voce@exemplo.com" />
          </div>
          <div className="form-field">
            <label htmlFor="register-password">Senha</label>
            <input id="register-password" name="password" type={showPassword ? 'text' : 'password'} autoComplete="new-password"
              required minLength={8} maxLength={256} aria-describedby="password-rules" />
            <p id="password-rules" className="field-help">Pelo menos 8 caracteres, com letra maiúscula, letra minúscula e número.</p>
          </div>
          <div className="form-field">
            <label htmlFor="register-confirmation">Confirmar senha</label>
            <input id="register-confirmation" name="confirmation" type={showPassword ? 'text' : 'password'} autoComplete="new-password"
              required minLength={8} maxLength={256} />
          </div>
          <label className="show-passwords"><input type="checkbox" checked={showPassword}
            onChange={(event) => setShowPassword(event.target.checked)} /> Mostrar senhas</label>
        </fieldset>
        {error && <p className="form-error" role="alert">{error}</p>}
        <button className="primary-button" type="submit" disabled={submitting}>{submitting ? 'Criando conta…' : 'Criar minha conta'}</button>
        <p className="submit-status" role="status">{submitting ? 'Enviando seu cadastro…' : ''}</p>
      </form>
      <p className="login-help">Já tem uma conta? <button className="access-link" type="button" disabled={submitting} onClick={onBack}>Entrar</button></p>
    </>
  )
}
