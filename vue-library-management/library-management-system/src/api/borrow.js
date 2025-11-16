import request from '@/utils/request'

// 分页查询借阅记录
export function queryPageApi(bookName, userName, status, page, pageSize) {
  return request({
    url: '/borrow/page',
    method: 'get',
    params: {
      bookName,
      userName,
      status,
      page,
      pageSize
    }
  })
}

// 查询所有用户
export function queryAllUsersApi() {
  return request({
    url: '/user/all',
    method: 'get'
  })
}

// 根据图书名称搜索图书
export function searchBooksApi(name) {
  return request({
    url: '/book/search',
    method: 'get',
    params: { name }
  })
}

// 借阅图书
export function borrowApi(borrowData) {
  return request({
    url: '/borrow',
    method: 'post',
    data: borrowData
  })
}

// 归还图书
export function returnApi(id) {
  return request({
    url: `/borrow/return/${id}`,
    method: 'put'
  })
}

// 续借图书
export function renewApi(id) {
  return request({
    url: `/borrow/renew/${id}`,
    method: 'put'
  })
}

// 删除借阅记录
export function deleteApi(id) {
  return request({
    url: `/borrow/${id}`,
    method: 'delete'
  })
}