import request from "../utils/request"

//登录
export const loginApi = (data) =>
  request.post('auth/login', data)