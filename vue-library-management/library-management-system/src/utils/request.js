import axios from 'axios'
import { ElMessage } from 'element-plus'
import router from '../router'

//创建axios实例对象
const request = axios.create({
  baseURL: '/api',
  timeout: 600000
})

//axios的请求 request 拦截器
request.interceptors.request.use(
  (config) => {
    const loginUser = JSON.parse(localStorage.getItem('loginUser'))
    if (loginUser && loginUser.token) {
      config.headers.token = loginUser.token
    }
    return config
  }
)

//axios的响应 response 拦截器
request.interceptors.response.use(
  (response) => { //成功回调
    console.log('=== 响应拦截器 ===')
    console.log('请求URL:', response.config.url)
    console.log('完整响应:', response)
    console.log('响应数据:', response.data)
    console.log('响应状态:', response.status)
    console.log('=================')
    return response.data
  },
  (error) => { //失败回调
    console.log('=== 错误响应 ===')
    console.log('错误详情:', error)
    console.log('错误响应:', error.response)
    console.log('=================')
    
    // 修复：检查 error.response 是否存在
    if (error.response && error.response.status === 401) {
      ElMessage.error('登录失效, 请重新登录');
      router.push('/login')
    } else {
      ElMessage.error('请求失败: ' + (error.message || '网络错误'));
    }
    return Promise.reject(error)
  }
)

export default request