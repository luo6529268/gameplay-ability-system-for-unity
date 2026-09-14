# 当前 Editor 桥接恢复

外部重启清掉了 Temp/Goal13_bridge.py。不要假设它存在，也不要启动第二个 Editor。当前项目 MCP 状态文件是 C:/Users/Logan/.unity-mcp/unity-mcp-status-b1b02287.json。

以下 Python 代码可通过 PowerShell here-string 传给 `D:/anaconda3/python.exe -X utf8 - <command> '<json>'`，或直接复用 functions store 的 `unityBridgeInline`。get_test_job 的超时/断线不是测试终态；重新查询同一 job。

```python
import json,socket,struct,sys
from pathlib import Path
s=json.loads(Path('C:/Users/Logan/.unity-mcp/unity-mcp-status-b1b02287.json').read_text(encoding='utf-8'))
assert s['project_name']=='gameplay-ability-system-for-unity' and s['unity_version']=='2022.3.62f3' and not s['reloading'], 'target editor is reloading or identity differs'
with socket.create_connection(('127.0.0.1',s['unity_port']),timeout=30) as c:
 c.settimeout(30)
 def read(n):
  b=b''
  while len(b)<n:
   p=c.recv(n-len(b))
   if not p:raise RuntimeError('connection closed')
   b+=p
  return b
 handshake=b''
 while not handshake.endswith(b'\n'):handshake+=read(1)
 assert handshake.startswith(b'WELCOME UNITY-MCP')
 b=json.dumps({'type':sys.argv[1],'params':json.loads(sys.argv[2])}).encode('utf-8')
 c.sendall(struct.pack('>Q',len(b))+b)
 print(read(struct.unpack('>Q',read(8))[0]).decode('utf-8'))
```
