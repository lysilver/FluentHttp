import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  vus: 100, // 指定要同时运行的虚拟用户数量
  duration: '100s', // 指定测试运行的总持续时间
};

export default function () {
  const res = http.get('http://localhost:9000/api/v1/order/k6/pool');
  // check(res, { 'status was 200': (r) => r.status == 200 });
  // sleep(1);
}
