# General

In the Triple .NET team we use [Git](https://git-scm.com) for source control management. How we Git - whether it is via a GUI or the CLI - is up to us. However, our commits must stick to the guidelines:

1. We must commit one change per commit
2. We must adhere to the commit title and -description requirements
3. We should not commit generated files
4. We should not commit personal user settings
5. We must include the .editorconfig in the root of our solutions

## Commit title and description

We should strive to make our commits concise and descriptive, allowing to understand what happens within by only reading its title. By starting our commits with the following keywords we communicate our intent of the change in a uniform and succinct manner. In the optional description, you can find space to freely describe your commit in more detail.

<table>
   <thead>
      <tr>
         <th>Keyword</th>
         <th>Description</th>
      </tr>
   </thead>
   <tbody>
      <tr>
         <td>Add</td>
         <td>Add code</td>
      </tr>
      <tr>
         <td>Bump</td>
         <td>Update one or more libraries</td>
      </tr>
      <tr>
         <td>Cut</td>
         <td>Deletes code</td>
      </tr>
      <tr>
         <td>Docs</td>
         <td>Changes to documentation</td>
      </tr>
      <tr>
         <td>Fix</td>
         <td>Fix an error</td>
      </tr>
      <tr>
         <td>Refactor</td>
         <td>Restructure existing code, without changing its external behavior</td>
      </tr>
      <tr>
         <td>Test</td>
         <td>Either adds or refactors a test</td>
      </tr>
   </tbody>
</table>

Table 1 - *Commit title keywords and their meaning*
