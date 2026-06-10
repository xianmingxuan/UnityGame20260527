
const express = require('express')
const app = express()
const port = 3000

app.use(express.static('StandaloneWindows64'))

app.listen(port, () => 
{
    console.log(`MyLocatServer Running - port:${port}`)
})