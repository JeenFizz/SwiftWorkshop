using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Photon.Pun;

public class ProductInfo
{
    public string name;
    public Color color;
    public List<string> machines;
    public int coef;
    public GameObject ui;
    public bool visible = false;
}

public class ProductFlowChart : MonoBehaviour
{
    private List<ProductInfo> ProductInfos { get; set; } = new List<ProductInfo>()
    {
        new ProductInfo(){ name = "A", color = Color.red, machines = new List<string>{ "F1", "F2", "T1", "P1", "F3", "G1" }, coef =  1 },
        new ProductInfo(){ name = "B", color = Color.yellow, machines =  new List<string>{ "G2", "T3", "F2", "F4", "P2" }, coef = 5 },
        new ProductInfo(){ name = "C", color = Color.black, machines = new List<string>{ "F3", "T2", "G1", "F4", "G2" }, coef = 3 },
        new ProductInfo(){ name = "D", color = Color.blue, machines = new List<string>{ "F1", "T1", "F3", "F2", "G2" }, coef = 2 },
        new ProductInfo(){ name = "E", color = Color.cyan, machines = new List<string>{ "F3", "G1", "F4", "P3" }, coef = 3 }
    };

    private List<(List<string> path, GameObject obj, string type)> CurrentProducts = new List<(List<string> path, GameObject, string)>();

    public float speed;

    public GameObject ProdutObject;
    private GameObject ProductPanel;
    public GameObject ProductInterface;
    private Dictionary<string, bool> ProductVisibility = new Dictionary<string, bool>() {
        { "A", true },
        { "B", true },
        { "C", true },
        { "D", true },
        { "E", true },
    };

    // Start is called before the first frame update
    void Start()
    {
        ProductPanel = GameObject.Find("ProductPanel");

        ProductInfos = ProductInfos.Select(pInfo =>
        {
            StartCoroutine(CreateProduct(pInfo));
            GameObject pInterface = Instantiate(ProductInterface, ProductPanel.transform);
            pInfo.ui = pInterface;

            pInterface.transform.Find("VisibilityButton").GetComponent<Image>().color = pInfo.color;
            pInterface.transform.Find("VisibilityButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                ProductVisibility[pInfo.name] = !ProductVisibility[pInfo.name];

                /*if(!ProductVisibility[pInfo.name])
                {
                    foreach (var p in CurrentProducts.Where(p => p.type == pInfo.name))
                    {
                        Destroy(p.obj);
                        ProductCountChange(p.type, -1);
                    }
                    CurrentProducts = CurrentProducts.Where(p => p.type != pInfo.name).ToList();
                }*/

                GetComponent<PhotonView>().RPC("ToggleProducts", RpcTarget.MasterClient, pInfo.name);

                //pInterface.transform.Find("VisibilityButton").transform.Find("VisibilityLabel").GetComponent<Text>().text = ProductVisibility[pInfo.name] ? "Toggle" : "Toggle";
            });
            pInterface.transform.Find("PathLabel").GetComponent<Text>().text = pInfo.machines.Aggregate("", (prev, next) => prev + " > " + next);
            pInterface.transform.Find("CoefLabel").GetComponent<Text>().text = pInfo.coef.ToString();

            return pInfo;
        }).ToList();
    }

    // Update is called once per frame
    void Update()
    {
        var newList = CurrentProducts.ToList();

        foreach (var product in CurrentProducts)
        {
            GameObject target = GameObject.Find(product.path.First());
            if (target == null || product.obj == null) continue;
            float step = speed * Time.deltaTime;
            Vector3 targetPos = target.transform.position;
            Vector3 currentPos = product.obj.transform.position;
            Vector3 newPos = Vector3.MoveTowards(currentPos, targetPos, step);
            product.obj.transform.position = newPos;

            if (Vector3.Distance(product.obj.transform.position, target.transform.position) < 0.1f)
            {
                product.path.RemoveAt(0);
                if(product.path.Count == 0)
                {
                    var productView = product.obj.GetComponent<PhotonView>();
                    productView.RPC("DeleteProduct", RpcTarget.MasterClient, productView.ViewID);
                    newList.Remove(product);
                    ProductCountChange(product.type, -1);
                }
            }
        }

        CurrentProducts = newList;
    }

    [PunRPC]
    public void DeleteProduct(int viewId)
    {
        var view = PhotonView.Find(viewId);
        view.RequestOwnership();
        PhotonNetwork.Destroy(view);
    }

    [PunRPC]
    public void ToggleProducts(string type)
    {
        if (!ProductVisibility[type])
        {
            foreach (var p in CurrentProducts.Where(p => p.type == type))
            {
                var view = p.obj.GetComponent<PhotonView>();
                view.RequestOwnership();
                PhotonNetwork.Destroy(view);
                ProductCountChange(p.type, -1);
            }
            CurrentProducts = CurrentProducts.Where(p => p.type != type).ToList();
        }
    }

    public IEnumerator CreateProduct(ProductInfo productInfo)
    {
        while (true)
        {
            yield return new WaitForSeconds(5 / productInfo.coef);

            GameObject machine = GameObject.Find(productInfo.machines.First());

            if (ProductVisibility[productInfo.name] && machine != null)
            {
                var pos = machine.transform.position;
                GetComponent<PhotonView>().RPC("SpawnProduct",
                    RpcTarget.MasterClient,
                    productInfo.color.r,
                    productInfo.color.g,
                    productInfo.color.b,
                    productInfo.name, 
                    productInfo.machines.ToArray(),
                    pos.x,
                    pos.y,
                    pos.z
                );
                ProductCountChange(productInfo.name, 1);
            }
        }
    }

    [PunRPC]
    public void SpawnProduct(float r, float g, float b, string name, string[] machines, float x, float y, float z)
    {
        GameObject productObject = PhotonNetwork.InstantiateSceneObject("Product", new Vector3(){x = x, y = y, z = z}, new Quaternion());

        foreach (TextMesh tmesh in productObject.transform.GetComponentsInChildren<TextMesh>()) tmesh.text = name;

        CurrentProducts.Add((machines.ToList(), productObject, name));

        /*var pView = productObject.GetComponent<PhotonView>();
        pView.RPC("SetProductColor", RpcTarget.AllBuffered, pView.ViewID, r, g, b);*/

        productObject.transform.GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }

    /*[PunRPC]
    public void SetProductColor(int viewID, float r, float g, float b)
    {
        PhotonView.Find(viewID).transform.GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }*/

    private void ProductCountChange(string name, int offset)
    {
        var t = ProductInfos.First(p => p.name == name).ui;
        var s = t.transform.Find("AmountLabel");
        var countLabel = ProductInfos.First(p => p.name == name).ui.transform.Find("AmountLabel").GetComponent<Text>();

        countLabel.text = (int.Parse(countLabel.text) + offset).ToString();
    }
}
