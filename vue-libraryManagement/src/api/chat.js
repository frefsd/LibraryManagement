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
  let buffer = ''
  
  try {
    while (true) {
      const { value, done } = await reader.read()
      
      if (done) {
        // 检查缓冲区是否还有剩余数据
        if (buffer.trim()) {
          processBuffer(buffer)
        }
        break
      }
      
      // 解码数据并添加到缓冲区
      buffer += decoder.decode(value, { stream: true })
      
      // 按行分割缓冲区
      const lines = buffer.split('\n')
      
      // 保留最后一行（可能不完整）
      buffer = lines.pop() || ''
      
      // 处理完整的行
      for (const line of lines) {
        processLine(line)
      }
    }
  } finally {
    reader.releaseLock()
  }

  function processLine(line) {
    // 跳过空行
    if (!line.trim()) return
    
    // SSE格式: data: {json}
    if (line.startsWith('data: ')) {
      const data = line.slice(6).trim()
      
      // 结束标记
      if (data === '[DONE]') {
        onChunk('') // 发送结束标记
        return
      }
      
      try {
        const parsed = JSON.parse(data)
        if (parsed && parsed !== '[DONE]') {
          onChunk(parsed, false)
        }
      } catch (error) {
        console.error('解析JSON失败:', error, '原始数据:', data)
      }
    }
  }
  
  function processBuffer(bufferContent) {
    const lines = bufferContent.split('\n')
    for (const line of lines) {
      processLine(line)
    }
  }
}