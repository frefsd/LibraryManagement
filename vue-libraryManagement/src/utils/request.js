import axios from 'axios'
import { ElMessage } from 'element-plus'
import router from '../router'


//创建axios实例对象
const request = axios.create({
  baseURL: 'https://localhost:7297',
  timeout: 15000 //15000毫秒 = 15秒
})

//axios的请求 request 拦截器
request.interceptors.request.use(
  (config) => {
    const loginUser = JSON.parse(localStorage.getItem('loginUser'))
    if (loginUser && loginUser.token) {
      //使用Authorization
      config.headers.Authorization = `Bearer ${loginUser.token}`
    }
    return config
  },
  (error) =>{
    return Promise.reject(error)
  }
)


// axios的响应 response 拦截器
request.interceptors.response.use(
  (response) => {
    return response.data
  },
  (error) => {
    console.log('=== 错误响应 ===')
    console.log('错误详情:', error)
    console.log('错误响应:', error.response)
    console.log('=================')

    // 优先使用后端返回的 msg
    let message = '请求失败'

    if (error.response) {
      const { status, data } = error.response

      if (data && data.msg) {
        message = data.msg
      } else if (status === 401) {
        message = '登录失效，请重新登录'
        router.push('/login')
      } else if (status >= 500) {
        message = '服务器内部错误，请稍后重试'
      } else if (status >= 400) {
        message = '请求参数错误或操作不被允许'
      }
    } else if (error.request) {
      message = '网络连接失败，请检查网络'
    } else {
      message = error.message || '未知错误'
    }

    ElMessage.error(message)
    return Promise.reject(error)
  }
)

export default request