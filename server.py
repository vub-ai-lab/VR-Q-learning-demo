from BaseHTTPServer import BaseHTTPRequestHandler
import SocketServer

PORT = 8080

import sys
import StringIO
import contextlib
import urllib

@contextlib.contextmanager
def stdoutIO(stdout=None):
    old = sys.stdout
    if stdout is None:
        stdout = StringIO.StringIO()
    sys.stdout = stdout
    yield stdout
    sys.stdout = old

class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        self.send_response(200)
        self.end_headers()

        command = urllib.unquote(self.path[1:]).replace('_', ' ')
        
        with stdoutIO() as s:
            try:
                exec command in globals()
            except Exception as e:
                print("error: " + str(e))
        self.wfile.write(s.getvalue())

SocketServer.TCPServer.allow_reuse_address = True
httpd = SocketServer.TCPServer(('localhost', PORT), Handler)
httpd.serve_forever()