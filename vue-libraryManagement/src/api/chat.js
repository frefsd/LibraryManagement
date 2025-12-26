
export const sendChatMessageStreamApi = async (message, onChunk) => {
  const response = await fetch('/chat/stream', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: (() => {
        const user = JSON.parse(localStorage.getItem('loginUser'))
        return user?.token ? `Bearer ${user.token}` : ''
      })()
    },
    body: JSON.stringify({ message })
  })

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`)
  }

  const reader = response.body.getReader()
  const decoder = new TextDecoder('utf-8')
  let done = false

  while (!done) {
    const { value, done: streamDone } = await reader.read()
    done = streamDone
    if (value) {
      const chunk = decoder.decode(value, { stream: true })
      onChunk(chunk) // 回调给 Vue 组件
    }
  }
}