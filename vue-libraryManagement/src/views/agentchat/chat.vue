<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import { v4 as uuidv4 } from 'uuid'
import { sendChatMessageStreamApi } from '../../api/chat'
import { ElMessageBox } from 'element-plus'


// refs
const messaggListRef = ref()
const isSending = ref(false)
const inputMessage = ref('')
const sessions = ref([]) // [{ id, title, messages: [] }]
const currentSessionId = ref(null)

// computed
const currentMessages = computed(() => {
  const session = sessions.value.find(s => s.id === currentSessionId.value)
  return session ? session.messages : []
})

// 工具函数
const generateSessionId = () => uuidv4().slice(0, 8)

const saveAllSessions = () => {
  localStorage.setItem('chat_sessions', JSON.stringify(sessions.value))
}

const loadAllSessions = () => {
  const saved = localStorage.getItem('chat_sessions')
  if (saved) {
    try {
      sessions.value = JSON.parse(saved)
    } catch (e) {
      console.warn('加载会话失败', e)
      sessions.value = []
    }
  }
}

const scrollToBottom = () => {
  nextTick(() => {
    if (messaggListRef.value) {
      messaggListRef.value.scrollTop = messaggListRef.value.scrollHeight
    }
  })
}

// 创建新会话（空对话）
const createNewSession = () => {
  const sessionId = generateSessionId()
  const newSession = {
    id: sessionId,
    title: '新对话',
    createdAt: new Date().toISOString(),
    messages: [
      {
        isUser: false,
        content: '你好！我是智慧图书助手，请问有什么可以帮您？',
        isTyping: false
      }
    ]
  }
  sessions.value.push(newSession)
  currentSessionId.value = sessionId
  saveAllSessions()
  nextTick(scrollToBottom) // 确保滚动到底部
}

// 切换会话
const switchSession = (sessionId) => {
  currentSessionId.value = sessionId
  scrollToBottom()
}

// 发送消息
const sendMessage = () => {
  if (!inputMessage.value.trim()) return

  const session = sessions.value.find(s => s.id === currentSessionId.value)
  if (!session) return

  sendRequestToSession(session, inputMessage.value.trim())
  inputMessage.value = ''
}

const clearAllSessions = () => {
  ElMessageBox.confirm('确定清空侧边栏中所有的历史会话？此操作不可恢复。', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(() => {
    sessions.value = []
    localStorage.removeItem('chat_sessions')
    createNewSession()
  }).catch(() => {
    console.log('取消清空')
  })
}

// 向指定会话发送请求
const sendRequestToSession = (session, message) => {
  isSending.value = true

  // 用户消息
  session.messages.push({ isUser: true, content: message, isTyping: false })

  // 自动设置标题：仅当是第一个用户消息且标题还是“新对话”
  if (session.title === '新对话') {
    // 取前20字符
    session.title = message.length > 20 ? message.substring(0, 20) + '...' : message
  }

  // 机器人占位消息
  const botMsg = { isUser: false, content: '', isTyping: true }
  session.messages.push(botMsg)

  let fullText = ''

  sendChatMessageStreamApi(message, (chunk) => {
    fullText += chunk
    botMsg.content = convertStreamOutput(fullText)
    scrollToBottom()
  })
    .then(() => {
      botMsg.isTyping = false
      isSending.value = false
      saveAllSessions() // 保存含新标题的会话
    })
    .catch((error) => {
      console.error('流式请求失败:', error)
      botMsg.content = '网络错误，请重试'
      botMsg.isTyping = false
      isSending.value = false
      saveAllSessions()
    })
}

const convertStreamOutput = (output) => {
  return output.replace(/\t/g, '    ')
}

// 初始化
onMounted(() => {
  loadAllSessions()

  if (sessions.value.length === 0) {
    createNewSession()
  } else {
    currentSessionId.value = sessions.value[0].id
    nextTick(scrollToBottom)
  }

  //补全已有会话的标题（兼容旧数据）
  let hasChanges = false
  sessions.value.forEach(session => {
    if (session.title === '新对话' || !session.title) {
      const firstUserMsg = session.messages.find(m => m.isUser)
      if (firstUserMsg) {
        session.title = firstUserMsg.content.length > 20
          ? firstUserMsg.content.substring(0, 20) + '...'
          : firstUserMsg.content
        hasChanges = true
      }
    }
  })

  if (hasChanges) {
    saveAllSessions() // 仅当有修改才保存，避免频繁写入
  }
})
</script>

<template>

  <div class="app-layout">
    <!-- 侧边栏：会话列表 -->
    <div class="sidebar">
      <div class="logo-section">
        <img src="@/assets/logo.png" alt="智慧图书" width="160" height="160" />
        <span class="logo-text">智慧图书</span>
      </div>

      <button class="sidebar-btn" @click="createNewSession">新对话</button>
      <button class="sidebar-btn" @click="clearAllSessions">清空所有</button>
      <div class="session-list">
        <div v-for="session in sessions" :key="session.id" class="session-item"
          :class="{ active: session.id === currentSessionId }" @click="switchSession(session.id)">
          <span>{{ session.title }}</span>
        </div>
      </div>
    </div>
    <!-- 主聊天区 -->
    <div class="main-content">
      <div class="chat-container">
        <div class="message-list" ref="messaggListRef">
          <div v-for="(message, index) in currentMessages" :key="index"
            :class="message.isUser ? 'message user-message' : 'message bot-message'">
            <i :class="message.isUser
              ? 'fa-solid fa-user message-icon'
              : 'fa-solid fa-robot message-icon'"></i>
            <span>
              <span v-text="message.content"></span>
              <span class="loading-dots" v-if="message.isTyping">
                <span class="dot"></span>
                <span class="dot"></span>
              </span>
            </span>
          </div>
        </div>

        <div class="input-container">
          <el-input v-model="inputMessage" placeholder="请输入消息" @keyup.enter="sendMessage"></el-input>
          <el-button @click="sendMessage" :disabled="isSending" type="primary">发送</el-button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.app-layout {
  display: flex;
  height: 100vh;
}

.sidebar {
  width: 220px;
  background-color: #f4f4f9;
  padding: 16px;
  display: flex;
  flex-direction: column;
  align-items: center;
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: #e0e0e0 transparent;
}

.sidebar::-webkit-scrollbar {
  width: 6px;
}

.sidebar::-webkit-scrollbar-track {
  background: transparent;
}

.sidebar::-webkit-scrollbar-thumb {
  background-color: rgba(224, 224, 224, 0.7);
  border-radius: 3px;
}

.sidebar::-webkit-scrollbar-thumb:hover {
  background-color: rgba(224, 224, 224, 0.9);
}

.logo-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 20px;
}

.logo-text {
  font-size: 18px;
  font-weight: bold;
  margin-top: 10px;
}

.sidebar-btn {
  width: 100%;
  padding: 8px 0;
  margin-bottom: 20px;
  background: #fff;
  border: 1px solid #dcdfe6;
  border-radius: 4px;
  color: #666;
  font-size: 14px;
  cursor: pointer;
  text-align: center;
}

.sidebar-btn:hover {
  background: #f5f5f5;
}

.session-list {
  width: 100%;
}

.session-item {
  padding: 8px 12px;
  margin-bottom: 8px;
  border-radius: 6px;
  cursor: pointer;
  background-color: #fff;
  transition: background-color 0.2s;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.session-item:hover {
  background-color: #e0e0e0;
}

.session-item.active {
  background-color: #d1e7ff;
  font-weight: bold;
}

.main-content {
  flex: 1;
  padding: 20px;
  overflow-y: auto;
}

.chat-container {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.message-list {
  flex: 1;
  overflow-y: auto;
  padding: 10px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  background-color: #fff;
  margin-bottom: 10px;
  display: flex;
  flex-direction: column;
}

.message {
  margin-bottom: 10px;
  padding: 10px;
  border-radius: 4px;
  display: flex;
  white-space: pre-wrap;
  word-break: break-word;
}

.user-message {
  max-width: 70%;
  background-color: #e1f5fe;
  align-self: flex-end;
  flex-direction: row-reverse;
}

.bot-message {
  max-width: 100%;
  background-color: #f1f8e9;
  align-self: flex-start;
}

.message-icon {
  margin: 0 10px;
  font-size: 1.2em;
}

.loading-dots {
  padding-left: 5px;
}

.dot {
  display: inline-block;
  margin-left: 5px;
  width: 8px;
  height: 8px;
  background-color: #000000;
  border-radius: 50%;
  animation: pulse 1.2s infinite ease-in-out both;
}

.dot:nth-child(2) {
  animation-delay: -0.6s;
}

@keyframes pulse {

  0%,
  100% {
    transform: scale(0.6);
    opacity: 0.4;
  }

  50% {
    transform: scale(1);
    opacity: 1;
  }
}

.input-container {
  display: flex;
}

.input-container .el-input {
  flex: 1;
  margin-right: 10px;
}
</style>