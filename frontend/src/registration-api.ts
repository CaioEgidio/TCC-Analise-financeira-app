export async function registerUser(email: string, password: string, confirmation: string): Promise<void> {
  if (password !== confirmation) throw new Error('As senhas não coincidem.')
  if (password.length < 8 || !/[0-9]/.test(password) || !/[a-z]/.test(password) || !/[A-Z]/.test(password)) {
    throw new Error('Use pelo menos 8 caracteres, com letra maiúscula, letra minúscula e número.')
  }
  let response: Response
  try {
    response = await fetch('/api/auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: email.trim(), password }),
    })
  } catch {
    throw new Error('A conexão foi interrompida. Tente entrar com os dados informados para conferir se a conta foi criada.')
  }
  const result: unknown = await response.json().catch(() => null)
  if (!response.ok) {
    if (result && typeof result === 'object' && 'errors' in result) {
      const errors = result.errors
      const messages = Array.isArray(errors) ? errors
        : errors && typeof errors === 'object' ? Object.values(errors).flat() : []
      const details = messages.filter((message): message is string => typeof message === 'string')
      if (details.length) throw new Error(details.join(' '))
    }
    throw new Error('Não foi possível criar a conta. Confira seus dados e tente novamente.')
  }
  if (!result || typeof result !== 'object' || !('id' in result) || typeof result.id !== 'string' || !result.id) {
    throw new Error('Não foi possível confirmar o cadastro. Tente entrar com os dados informados antes de cadastrar novamente.')
  }
}
