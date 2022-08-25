http://mousouprogrammer.blogspot.com/2015/05/croslynsyntaxwalker.html

C#ソースからメソッドコメント・クラスコメント等を取得したい

Roslynでコメントを取る方法は？  
VisitMethodDeclarationのような、構文解析時にメソッドを通る時の処理用のメソッドがあるので、その処理をoverrideして取得する。
summaryとかはXMLなので、XML解析をする。

https://stackoverflow.com/questions/49843885/roslyn-get-grouped-single-line-comments
通常コメントは1行ずつしか取れないみたいなので、検出は難しそう。
